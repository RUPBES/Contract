using Dapper;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.KDO;
using DatabaseLayer.RepositoriesDapper.Sql;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Diagnostics;

namespace DatabaseLayer.RepositoriesDapper.ViewRepo
{
    public class VContractDpRepository : IReadonlyRepoDapper<VContract>
    {
        private readonly string? _connectionString = null;
        private readonly string sqlStrStart = @$" SELECT c.Id ,c.Number ,c.[ProcedureName] ,c.[SignatoryEmp] ,c.[ResponsibleEmp] ,c.[GenContractor]
                       ,c.[Client] ,c.[ResponsibleForWork] ,c.[SubContractId] ,c.[AgreementContractId] ,c.[Date] ,c.[ContractTerm] 
                        ,c.[Сurrency] ,c.[WorkType] ,c.[NameObject] ,c.[FundingSource] ,c.[PaymentСonditionsAvans] ,c.[PaymentСonditionsRaschet]
                        ,c.[Author] ,c.[Owner] ,c.[IsClosed] ,c.[IsExpired] ,c.[IsArchive] ,c.[PreYearSum] ,c.[RemainingSum] ,c.[ThisYearSum] , c.ProcedureId
                        , COALESCE(a.LastAmendmentPrice, c.ContractPrice) AS ContractPrice
                        ,COALESCE(a.DateBeginWork, c.DateBeginWork) AS DateBeginWork   
                        ,COALESCE(a.DateEndWork, c.DateEndWork) AS DateEndWork  
                        ,COALESCE(a.DateEntryObject, c.EnteringTerm) AS EnteringTerm  

                        FROM vContracts c
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

        public VContractDpRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public int Count(string[]? orgList)
        {
            if (orgList is null || orgList?.Length == 0)
            {  
                using (IDbConnection db = new SqlConnection(_connectionString))
                {
                    return db.Query<int>("SELECT COUNT(Id) FROM vContracts").FirstOrDefault();
                }
            }          

            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return db.Query<int>($"SELECT COUNT(Id) FROM vContracts CROSS APPLY STRING_SPLIT(Owner, ',') owners WHERE (Author IN @orgList or owners.value IN @orgList)", new { orgList }).FirstOrDefault();
            }
        }

        public int Count(string predicate, string? databaseName)
        {
            string viewName = DbQualifier.Qualify(databaseName, "vContracts");
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return db.Query<int>($"SELECT COUNT(Id) FROM {viewName}").FirstOrDefault();
            }
        }

        public IEnumerable<VContract> Find(string predicate)
        {
            if (string.IsNullOrEmpty(predicate))
            {
                return Array.Empty<VContract>();
            }

            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return db.Query<VContract>($"{sqlStrStart} {predicate}").ToList();
            }
        }

        public IEnumerable<VContract> Find(string predicate, string? databaseName)
        {
            if (string.IsNullOrEmpty(predicate))
            {
                return Array.Empty<VContract>();
            }

            string qualified = DbQualifier.Qualify(databaseName, "vContracts");
            string sqlStart = sqlStrStart.Replace("FROM vContracts c", $"FROM {qualified} c");
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return db.Query<VContract>($"{sqlStart} {predicate}").ToList();
            }
        }

        public IEnumerable<VContract> Find(string predicate, string[] orgList)
        {
            if (string.IsNullOrEmpty(predicate))
            {
                return Array.Empty<VContract>();
            }
            var sql = @$"{sqlStrStart}
                        CROSS APPLY STRING_SPLIT(c.Owner, ',') owners
                        WHERE (Author IN @orgList or owners.value IN @orgList)";

            using (IDbConnection db = new SqlConnection(_connectionString))
            {                
                return db.Query<VContract>($"{sql} {predicate}", new {orgList}).ToList();
            }
        }

        public IEnumerable<VContract> Find(string predicate, string[] orgList, string? databaseName)
        {
            if (string.IsNullOrEmpty(predicate))
            {
                return Array.Empty<VContract>();
            }

            string viewName = DbQualifier.Qualify(databaseName, "vContracts");
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return db.Query<VContract>($"SELECT * FROM {viewName} c {predicate}", new { orgList }).ToList();
            }
        }

        public IEnumerable<VContract> GetAll()
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return db.Query<VContract>(sqlStrStart).ToList();
            }
        }

        public IEnumerable<VContract> GetAll(string? databaseName)
        {
            string qualified = DbQualifier.Qualify(databaseName, "vContracts");
            string sqlStart = sqlStrStart.Replace("FROM vContracts c", $"FROM {qualified} c");
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return db.Query<VContract>(sqlStart).ToList();
            }
        }

        public VContract GetById(int id)
        {
            if (id > 0)
            {
                var sql = @$"{sqlStrStart} WHERE c.Id = @id";
               
                using (IDbConnection db = new SqlConnection(_connectionString))
                {
                    return db.Query<VContract>(sql, new { id }).FirstOrDefault();
                }
            }
            return null;
        }

        public VContract GetById(int id, string? databaseName)
        {
            if (id > 0)
            {
                var sqlStart = sqlStrStart.Replace("FROM vContracts c", $"FROM {DbQualifier.Qualify(databaseName, "vContracts")} c");
                var sql = @$"{sqlStart} WHERE c.Id = @id";

                using (IDbConnection db = new SqlConnection(_connectionString))
                {
                    return db.Query<VContract>(sql, new { id }).FirstOrDefault();
                }
            }
            return null;
        }

        public IEnumerable<VContract> GetEntitySkipTake(int skip, int take, string org)
        {
            string[] orgList = org.Split(',');

            var sql = @$"{sqlStrStart}
                        CROSS APPLY STRING_SPLIT(c.Owner, ',') owners
                        WHERE (Author IN @orgList or owners.value IN @orgList)
                        ORDER BY Id DESC
                        OFFSET @skip ROWS
                        FETCH NEXT @take ROWS ONLY";

            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return db.Query<VContract>(sql, new { skip, take, orgList }).ToList();
            }
        }

        public IEnumerable<VContract> GetEntitySkipTake(int skip, int take, string org, string? databaseName)
        {
            string[] orgList = org.Split(',');

            var sqlStart = sqlStrStart.Replace("FROM vContracts c", $"FROM {DbQualifier.Qualify(databaseName, "vContracts")} c");
            var sql = @$"{sqlStart}
                        CROSS APPLY STRING_SPLIT(c.Owner, ',') owners
                        WHERE (Author IN @orgList or owners.value IN @orgList)
                        ORDER BY Id DESC
                        OFFSET @skip ROWS
                        FETCH NEXT @take ROWS ONLY";

            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return db.Query<VContract>(sql, new { skip, take, orgList }).ToList();
            }
        }

        
    }
}
