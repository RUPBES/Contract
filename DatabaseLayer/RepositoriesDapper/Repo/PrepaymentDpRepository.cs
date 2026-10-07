using Dapper;
using DatabaseLayer.Interfaces.Dapper;
using DatabaseLayer.Models.EXTRA;
using DatabaseLayer.Models.KDO;
using DatabaseLayer.RepositoriesDapper.Sql;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Diagnostics.Contracts;
using File = DatabaseLayer.Models.KDO.File;

namespace DatabaseLayer.RepositoriesDapper.Repo;

public class PrepaymentDpRepository : IReadonlyPrepaymentDapperRepo
{
    private readonly string? _connectionString = null;

    private string PeriodsPrepSql = @"
        DECLARE @StartPeriod DATE = @StartPeriodParam;
        DECLARE @EndPeriod   DATE = @EndPeriodParam;
DECLARE @EndPeriodLast DATE;
 

        -- Нормализуем обе даты до 1-го числа месяца
        SET @StartPeriod = DATEFROMPARTS(YEAR(@StartPeriod), MONTH(@StartPeriod), 1);
        SET @EndPeriod   = DATEFROMPARTS(YEAR(@EndPeriod),   MONTH(@EndPeriod),   1);
  -- Отдельно считаем последний день конечного месяца — для фильтрации данных
    SET @EndPeriodLast = EOMONTH(@EndPeriod);

        ;WITH Periods AS (
            SELECT @StartPeriod AS PeriodDate
            UNION ALL
            SELECT DATEADD(MONTH, 1, PeriodDate)
            FROM Periods
            WHERE PeriodDate < @EndPeriod               -- конец не включается
        ),
        PrepaymentData AS (
            SELECT
                p.Id            AS PrepId,
                pp.Period,
                ISNULL(a.Id, 0) AS AmendmentId,          -- 0 = исходный договор (ДС нет)
                a.Number        AS AmendNumber,
                pp.CurrentValue AS CurrentAmount,
                pp.TargetValue  AS TargetAmount
            FROM Prepayment p
            LEFT JOIN PrepaymentAmendment ap ON p.Id = ap.PrepaymentId
            LEFT JOIN Amendment a            ON a.Id = ap.AmendmentId
            JOIN PrepaymentPlan pp           ON pp.PrepaymentId = p.Id
            WHERE p.ContractId = @ContractId
              AND pp.Period >= @StartPeriod
              AND pp.Period <= @EndPeriodLast
        ),
        TakeData AS (
            SELECT
                p.Period,
                p.CurrentValue AS PrepFactCurrent,
                p.TargetValue  AS PrepFactTarget
            FROM PrepaymentReceived p           
            WHERE p.ContractId = @ContractId
              AND p.Period >= @StartPeriod
              AND p.Period <= @EndPeriodLast
        ),
        FormData AS (
            SELECT
                f.Period,
                f.OffsetCurrentPrepayment AS FactCurrent,
                f.OffsetTargetPrepayment AS FactTarget,   
                files.Files
            FROM [FormC3A] f
            OUTER APPLY (
                SELECT STRING_AGG(
                    CAST(CAST(fa.Id AS NVARCHAR(20)) + '**' + fa.FileName + '**' + fa.FilePath AS NVARCHAR(MAX)),
                    '***'
                ) AS Files
                FROM FormFile ff
                JOIN [File] fa ON fa.Id = ff.FileId
                WHERE ff.FormId = f.Id
            ) files
            WHERE f.ContractId = @ContractId
              AND f.IsOwnForces = 0
              AND f.Period >= @StartPeriod
              AND f.Period <= @EndPeriodLast
        )
        SELECT         
            pr.PeriodDate AS Period,
            pd.PrepId,
            pd.AmendmentId,
            pd.AmendNumber,
            pd.CurrentAmount,
            pd.TargetAmount,
            td.PrepFactCurrent,
            td.PrepFactTarget,
            fd.FactCurrent,
            fd.FactTarget,
            fd.Files
        FROM Periods pr
        LEFT JOIN PrepaymentData pd
            ON pd.Period >= pr.PeriodDate
           AND pd.Period <  DATEADD(MONTH, 1, pr.PeriodDate)
        LEFT JOIN TakeData td
            ON td.Period >= pr.PeriodDate
           AND td.Period <  DATEADD(MONTH, 1, pr.PeriodDate)
        LEFT JOIN FormData fd
            ON fd.Period >= pr.PeriodDate
           AND fd.Period <  DATEADD(MONTH, 1, pr.PeriodDate)
        ORDER BY pr.PeriodDate
        OPTION (MAXRECURSION 0);
        ";

    private string AdvanceSummaryPrepSql = @"
                           SELECT
    prep.Total,
    prepFact.Received,
    ISNULL(settled.Settled, 0) AS Settled,
    (prepFact.Received - ISNULL(settled.Settled, 0)) AS Unsettled
FROM Prepayment p
OUTER APPLY (
    SELECT SUM(pp.CurrentValue) AS Total
    FROM PrepaymentPlan pp
    WHERE pp.PrepaymentId = p.Id
) prep
OUTER APPLY (
   SELECT SUM(p2.CurrentValue) AS Received
    FROM PrepaymentReceived p2    
    WHERE p2.ContractId = @ContractId
) prepFact
OUTER APPLY (
    SELECT SUM(f.OffsetCurrentPrepayment) AS Settled
    FROM [FormC3A] f
    WHERE f.ContractId = @ContractId
      AND f.IsOwnForces = 0
) settled
WHERE p.ContractId = @ContractId
  AND p.Id = (
      SELECT TOP 1 p3.Id
      FROM Prepayment p3
      WHERE p3.ContractId = @ContractId
      ORDER BY p3.Id DESC
  );
                    
            SELECT
    prep.Total,
    prepFact.Received,
    ISNULL(settled.Settled, 0) AS Settled,
    (prepFact.Received - ISNULL(settled.Settled, 0)) AS Unsettled
FROM Prepayment p
OUTER APPLY (
    SELECT SUM(pp.TargetValue) AS Total
    FROM PrepaymentPlan pp
    WHERE pp.PrepaymentId = p.Id
) prep
OUTER APPLY (
    SELECT SUM(p2.TargetValue) AS Received
    FROM PrepaymentReceived p2
    WHERE p2.ContractId = @ContractId
) prepFact
OUTER APPLY (
    SELECT SUM(f.OffsetTargetPrepayment) AS Settled
    FROM [FormC3A] f
    WHERE f.ContractId = @ContractId
      AND f.IsOwnForces = 0
) settled
WHERE p.ContractId = @ContractId
  AND p.Id = (
      SELECT TOP 1 p3.Id
      FROM Prepayment p3
      WHERE p3.ContractId = @ContractId
      ORDER BY p3.Id DESC
  );";
    public PrepaymentDpRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public int Count(string[] orgList, string? databaseName)
    {
        string table = DbQualifier.Qualify(databaseName, "Prepayment");

        if (orgList is null || orgList?.Length == 0)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return db.Query<int>($"SELECT COUNT(Id) FROM {table}").FirstOrDefault();
            }
        }

        string joinContract = DbQualifier.Qualify(databaseName, "Contract");

        using (IDbConnection db = new SqlConnection(_connectionString))
        {
            return db.Query<int>(@$"SELECT distinct COUNT(Id) FROM {table} c
									LEFT JOIN {joinContract} cc ON c.ContractId = cc.Id 
									CROSS APPLY STRING_SPLIT(cc.Owner, ',') owners
									WHERE (cc.Author IN @orgList or owners.value IN @orgList)", new { orgList }).FirstOrDefault();
        }
    }

    public IEnumerable<Prepayment> Find(string predicate, string[] orgList, string? databaseName)
    {
        if (string.IsNullOrEmpty(predicate))
        {
            return Array.Empty<Prepayment>();
        }

        string table = DbQualifier.Qualify(databaseName, "Prepayment");
        string joinContract = DbQualifier.Qualify(databaseName, "Contract");

        var sql = @$"SELECT distinct c.* FROM {table} c
					LEFT JOIN {joinContract} cc ON c.ContractId = cc.Id
					CROSS APPLY STRING_SPLIT(cc.Owner, ',') owners
					WHERE (cc.Author IN @orgList or owners.value IN @orgList) {predicate}";

        using (IDbConnection db = new SqlConnection(_connectionString))
        {
            return db.Query<Prepayment>(sql, new { orgList }).ToList();
        }
    }

    public IEnumerable<Prepayment> GetAll(string? databaseName)
    {
        string table = DbQualifier.Qualify(databaseName, "Prepayment");
        using (IDbConnection db = new SqlConnection(_connectionString))
        {
            return db.Query<Prepayment>(@$"SELECT c.* FROM {table} c").ToList();
        }
    }

    public Prepayment GetById(int id, string? databaseName)
    {
        if (id > 0)
        {
            string table = DbQualifier.Qualify(databaseName, "Prepayment");

            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return db.Query<Prepayment>(@$"SELECT c.* FROM {table} c WHERE c.Id = @id", new { id }).FirstOrDefault();
            }
        }
        return null;
    }

    public IEnumerable<Prepayment> GetEntitySkipTake(int skip, int take, string organizationName, string? databaseName)
    {
        string table = DbQualifier.Qualify(databaseName, "Prepayment");
        string joinContract = DbQualifier.Qualify(databaseName, "Contract");

        var sql = @$"SELECT distinct c.* FROM {table} c
					LEFT JOIN {joinContract} cc ON c.ContractId = cc.Id
					CROSS APPLY STRING_SPLIT(cc.Owner, ',') owners
					WHERE (cc.Author IN @orgList or owners.value IN @orgList)
					ORDER BY c.Id DESC
					OFFSET @skip ROWS
					FETCH NEXT @take ROWS ONLY";

        string[] orgList = organizationName.Split(',');

        using (IDbConnection db = new SqlConnection(_connectionString))
        {
            return db.Query<Prepayment>(sql, new { skip, take, orgList }).ToList();
        }
    }

    public async Task<List<PeriodAdvance>> GetPeriodAdvancesAsync(int contractId, DateTime startPeriod, DateTime endPeriod, string? databaseName)
    {
        if (!string.IsNullOrEmpty(databaseName))
        {
            PeriodsPrepSql = $"use {databaseName}; {PeriodsPrepSql}";
        }

        using (IDbConnection db = new SqlConnection(_connectionString))
        {
            var rows = await db.QueryAsync<PeriodAdvanceRow>(
                PeriodsPrepSql,
                new
                {
                    StartPeriodParam = startPeriod,
                    EndPeriodParam = endPeriod,
                    ContractId = contractId
                });

            var result = rows
                .GroupBy(r => r.Period)
                .OrderBy(g => g.Key)
                .Select(g =>
                {
                    // PrepFact/Fact/Files не зависят от версии ДС, поэтому одинаковы для всех строк
                    // группы — берём из первой строки периода.
                    var first = g.First();

                    var versions = g
                        .Where(r => r.AmendmentId.HasValue) // на периоде может не быть ни одной версии
                        .Select(r => new PeriodVersion
                        {
                            AmendmentId = r.AmendmentId!.Value,
                            AmendNumber = r.AmendNumber,
                            CurrentAmount = r.CurrentAmount ?? 0m,
                            TargetAmount = r.TargetAmount ?? 0m
                        })
                        .ToList();

                    return new PeriodAdvance
                    { 
                        Period = g.Key,
                        PrepId = g.Last().PrepId, // последний аванс по ДС или без
                        Versions = versions,
                        PrepFactCurrent = first.PrepFactCurrent,
                        PrepFactTarget = first.PrepFactTarget,
                        FactCurrent = first.FactCurrent,
                        FactTarget = first.FactTarget,
                        Files = ParseFiles(first.Files)
                    };
                })
                .ToList();

            return result;
        }
    }

    public async Task<PrepaymentSchedule?> GetTotalsAsync(int contractId, string? databaseName)
    {
        if (!string.IsNullOrEmpty(databaseName))
        {
            AdvanceSummaryPrepSql = $"use {databaseName}; {AdvanceSummaryPrepSql}";
        }

        using (IDbConnection db = new SqlConnection(_connectionString))
        {
            using var multi = await db.QueryMultipleAsync(
             AdvanceSummaryPrepSql,
             new { ContractId = contractId });

            var current = await multi.ReadFirstOrDefaultAsync<AdvanceSummary>();
            var target = await multi.ReadFirstOrDefaultAsync<AdvanceSummary>();

            return new PrepaymentSchedule
            {
                CurrentSummary = current ?? new AdvanceSummary(),
                TargetSummary = target ?? new AdvanceSummary()
            };
        }
    }



    /// <summary>
    /// Разбирает строку вида "Id**FileName**FilePath***Id**FileName**FilePath..." в список файлов.
    /// </summary>
    private static List<File> ParseFiles(string? raw)
    {
        var result = new List<File>();
        if (string.IsNullOrWhiteSpace(raw))
        {
            return result;
        }

        var entries = raw.Split(new[] { "***" }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var entry in entries)
        {
            var parts = entry.Split(new[] { "**" }, StringSplitOptions.None);
            if (parts.Length != 3)
            {
                continue; // защита от неожиданного формата
            }

            if (!int.TryParse(parts[0], out var id))
            {
                continue;
            }

            result.Add(new File
            {
                Id = id,
                FileName = parts[1],
                FilePath = parts[2]
            });
        }

        return result;
    }

    internal class PeriodAdvanceRow
    {
        public DateTime Period { get; set; }

        // null => для данного периода вообще нет строки PrepaymentPlan (ни одной версии)
  
        public int PrepId { get; set; }
        public int? AmendmentId { get; set; }
        public string? AmendNumber { get; set; }
        public decimal? CurrentAmount { get; set; }
        public decimal? TargetAmount { get; set; }

        public decimal? PrepFactCurrent { get; set; }
        public decimal? PrepFactTarget { get; set; }

        public decimal? FactCurrent { get; set; }
        public decimal? FactTarget { get; set; }

        public string? Files { get; set; }
    }
}