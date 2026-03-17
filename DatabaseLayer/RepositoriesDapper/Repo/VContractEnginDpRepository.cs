using Dapper;
using DatabaseLayer.Interfaces.Dapper;
using DatabaseLayer.Models.KDO;
using DatabaseLayer.RepositoriesDapper.Sql;
using Microsoft.Data.SqlClient;
using System.Data;

namespace DatabaseLayer.RepositoriesDapper.Repo
{
    public class VContractEnginDpRepository : IReadonlyRepoDapper<VContractEngin>
    {
        private readonly string? _connectionString = null;
        private readonly string sqlStrStart = @$" SELECT distinct c.Id ,c.Number ,c.[ProcedureName]  ,c.[GenContractor],c.[Client] ,c.[IsEngineering] ,c.[Date] ,c.[ContractTerm] 
                        ,c.[Сurrency] ,c.[NameObject] ,c.[FundingSource] ,c.[PaymentСonditionsAvans] ,c.[PaymentСonditionsRaschet] ,c.[PaymentСonditionsPrice]
                        ,c.[Author] ,c.[Owner] ,c.[IsClosed] ,c.[IsExpired] ,c.[IsArchive] ,c.[PreYearSum] ,c.[RemainingSum] ,c.[ThisYearSum] , c.ProcedureId
                        , COALESCE(a.LastAmendmentPrice, c.ContractPrice) AS ContractPrice
                        ,COALESCE(a.DateBeginWork, c.DateBeginWork) AS DateBeginWork   
                        ,COALESCE(a.DateEndWork, c.DateEndWork) AS DateEndWork  
                        ,COALESCE(a.DateEntryObject, c.EnteringTerm) AS EnteringTerm  
                        ,c.WorkflowRef

                        FROM vContractEngin c
                        OUTER APPLY (
                            SELECT TOP 1         
                                ContractPrice AS LastAmendmentPrice,
                                DateBeginWork,
                                DateEndWork,
                                DateEntryObject
                            FROM Amendment a
                            WHERE ContractId = c.Id
                            ORDER BY a.[Date] DESC
                        ) a";

        public VContractEnginDpRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public int Count(string[]? orgList, string? databaseName)
        {
            string viewName = DbQualifier.Qualify(databaseName, "vContractEngin");

            if (orgList is null || orgList?.Length == 0)
            {
                using (IDbConnection db = new SqlConnection(_connectionString))
                {
                    return db.Query<int>($"SELECT distinct COUNT(Id) FROM {viewName}").FirstOrDefault();
                }
            }

            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return db.Query<int>($"SELECT distinct COUNT(Id) FROM {viewName} CROSS APPLY STRING_SPLIT(Owner, ',') owners WHERE (Author IN @orgList or owners.value IN @orgList)", new { orgList }).FirstOrDefault();
            }
        }

        public IEnumerable<VContractEngin> Find(string predicate, string[] orgList, string? databaseName)
        {
            if (string.IsNullOrEmpty(predicate))
            {
                return Array.Empty<VContractEngin>();
            }

            string qualified = DbQualifier.Qualify(databaseName, "vContractEngin");
            string sqlStart = sqlStrStart.Replace("FROM vContracts c", $"FROM {qualified} c");

            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return db.Query<VContractEngin>($"{sqlStart} CROSS APPLY STRING_SPLIT(c.Owner, ',') owners WHERE (Author IN @orgList or owners.value IN @orgList) {predicate}", new { orgList }).ToList();
            }
        }

        public IEnumerable<VContractEngin> GetAll(string? databaseName)
        {
            string qualified = DbQualifier.Qualify(databaseName, "vContractEngin");
            string sqlStart = sqlStrStart.Replace("FROM vContracts c", $"FROM {qualified} c");
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return db.Query<VContractEngin>(sqlStart).ToList();
            }
        }

        public VContractEngin GetById(int id, string? databaseName)
        {
            if (id > 0)
            {
                var sqlStart = sqlStrStart.Replace("FROM vContracts c", $"FROM {DbQualifier.Qualify(databaseName, "vContractEngin")} c");
                var sql = @$"{sqlStart} WHERE c.Id = @id";

                using (IDbConnection db = new SqlConnection(_connectionString))
                {
                    return db.Query<VContractEngin>(sql, new { id }).FirstOrDefault();
                }
            }
            return null;
        }

        public IEnumerable<VContractEngin> GetEntitySkipTake(int skip, int take, string org, string? databaseName)
        {
            string[] orgList = org.Split(',');

            var sqlStart = sqlStrStart.Replace("FROM vContractEngin c", $"FROM {DbQualifier.Qualify(databaseName, "vContractEngin")} c");
            var sql = @$"{sqlStart}
                        CROSS APPLY STRING_SPLIT(c.Owner, ',') owners
                        WHERE (Author IN @orgList or owners.value IN @orgList)
                        ORDER BY Id DESC
                        OFFSET @skip ROWS
                        FETCH NEXT @take ROWS ONLY";

            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return db.Query<VContractEngin>(sql, new { skip, take, orgList }).ToList();
            }
        }


        /// <summary>
        /// Фильтрация и сортировка данных
        /// </summary>
        /// <param name="skip">Количество записей которые пропускаем</param>
        /// <param name="take">Количество записей которые забираем</param>
        /// <param name="org">Строка с названиями организаций, данные которых видны для пользователя</param>
        /// <param name="query">Строка с условием запроса ( "начинаться должна с "and ", после чего условие помещается в скобки "(....)")</param>
        /// <param name="databaseName">название БД, если необходимо взять данные, например из архивной БД</param>
        /// <returns></returns>
        public (IEnumerable<VContractEngin>, int) Filter(int skip, int take, string org, string? query, string? orderBy, string? databaseName)
        {
            string[] orgList = org.Split(',');
            var sqlStart = sqlStrStart.Replace("FROM vContractEngin c", $"FROM {DbQualifier.Qualify(databaseName, "vContractEngin")} c");
            var sql = @$"
                        SELECT distinct COUNT(DISTINCT c.Id) FROM {DbQualifier.Qualify(databaseName, "vContractEngin")} c 
                        OUTER APPLY (
                            SELECT TOP 1         
                                ContractPrice AS LastAmendmentPrice,
                                DateBeginWork,
                                DateEndWork,
                                DateEntryObject
                            FROM Amendment a
                            WHERE ContractId = c.Id
                            ORDER BY a.[Date] DESC
                        ) a
                        CROSS APPLY STRING_SPLIT(c.Owner, ',') owners
                        WHERE (Author IN @orgList or owners.value IN @orgList)
                        {query};

                        {sqlStart}
                        CROSS APPLY STRING_SPLIT(c.Owner, ',') owners
                        WHERE (Author IN @orgList or owners.value IN @orgList)
                        {query} {orderBy}
                        OFFSET @skip ROWS
                        FETCH NEXT @take ROWS ONLY;";

            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                var multi = db.QueryMultiple(sql, new { skip, take, orgList });
                var totalCount = multi.ReadSingle<int>();
                var contracts = multi.Read<VContractEngin>().ToList();
                return (contracts, totalCount);
            }
        }
    }
}
