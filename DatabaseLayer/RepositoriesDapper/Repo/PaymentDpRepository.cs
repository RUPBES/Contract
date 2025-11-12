using Dapper;
using DatabaseLayer.Interfaces.Entities;
using DatabaseLayer.Models.KDO;
using Microsoft.Data.SqlClient;
using DatabaseLayer.RepositoriesDapper.Sql;
using System.Data;

namespace DatabaseLayer.RepositoriesDapper.Repo
{
    public class PaymentDpRepository : IReadonlyPaymentDapperRepo
    {
        string _connectionString = null;
        private readonly string sqlStrStart = @$"SELECT * FROM VPayableCash c";

        public PaymentDpRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public int Count()
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return db.Query<int>("SELECT COUNT(Id) FROM VPayableCash").FirstOrDefault();
            }
        }

        public int Count(string? databaseName)
        {
            string viewName = DbQualifier.Qualify(databaseName, "VPayableCash");
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return db.Query<int>($"SELECT COUNT(Id) FROM {viewName}").FirstOrDefault();
            }
        }

        public IEnumerable<VPaymentCash> Find(string predicate)
        {
            if (string.IsNullOrEmpty(predicate))
            {
                return Array.Empty<VPaymentCash>();
            }

            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return db.Query<VPaymentCash>($"SELECT * FROM VPayableCash c {predicate}").ToList();
            }
        }

        public IEnumerable<VPaymentCash> Find(string predicate, string? databaseName)
        {
            if (string.IsNullOrEmpty(predicate))
            {
                return Array.Empty<VPaymentCash>();
            }

            string sqlStart = sqlStrStart.Replace("FROM VPayableCash c", $"FROM {DbQualifier.Qualify(databaseName, "VPayableCash")} c");
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return db.Query<VPaymentCash>($"{sqlStart} {predicate}").ToList();
            }
        }

        public IEnumerable<VPaymentCash> Find(string predicate, string[] orgList)
        {
            //if (string.IsNullOrEmpty(predicate))
            //{
            //    return Array.Empty<VPaymentCash>();
            //}

            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return db.Query<VPaymentCash>($"SELECT * FROM VPayableCash c CROSS APPLY STRING_SPLIT(c.Owner, ',') owners WHERE (Author IN @orgList or owners.value IN @orgList) {predicate}", new { orgList }).ToList();
            }
        }

        public IEnumerable<VPaymentCash> Find(string predicate, string[] orgList, string? databaseName)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                string viewName = DbQualifier.Qualify(databaseName, "VPayableCash");
                return db.Query<VPaymentCash>($"SELECT * FROM {viewName} c CROSS APPLY STRING_SPLIT(c.Owner, ',') owners WHERE (Author IN @orgList or owners.value IN @orgList) {predicate}", new { orgList }).ToList();
            }
        }

        public IEnumerable<VPaymentCash> GetAll()
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return db.Query<VPaymentCash>(@$"SELECT * FROM VPayableCash c").ToList();
            }
        }

        public IEnumerable<VPaymentCash> GetAll(string? databaseName)
        {
            string sqlStart = sqlStrStart.Replace("FROM VPayableCash c", $"FROM {DbQualifier.Qualify(databaseName, "VPayableCash")} c");
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return db.Query<VPaymentCash>(sqlStart).ToList();
            }
        }

        public VPaymentCash GetById(int id)
        {
            if (id > 0)
            {
                using (IDbConnection db = new SqlConnection(_connectionString))
                {
                    return db.QueryFirstOrDefault<VPaymentCash>(@$"SELECT * FROM VPayableCash c where c.Id = @id", new { id });
                }
            }
            return null;
        }

        public VPaymentCash GetById(int id, string? databaseName)
        {
            if (id > 0)
            {
                string viewName = DbQualifier.Qualify(databaseName, "VPayableCash");
                using (IDbConnection db = new SqlConnection(_connectionString))
                {
                    return db.QueryFirstOrDefault<VPaymentCash>(@$"SELECT * FROM {viewName} c where c.Id = @id", new { id });
                }
            }
            return null;
        }

        public IEnumerable<VPaymentCash> GetEntitySkipTake(int skip, int take, string org)
        {
            var sql = @$"SELECT * FROM VPayableCash c
                        CROSS APPLY STRING_SPLIT(c.Owner, ',') owners
                        WHERE (Author IN @orgList or owners.value IN @orgList)
                        ORDER BY Id DESC
                        OFFSET @skip ROWS
                        FETCH NEXT @take ROWS ONLY";

            string[] orgList = org.Split(',');

            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return db.Query<VPaymentCash>(sql, new { skip, take, orgList }).ToList();
            }
        }

        public IEnumerable<VPaymentCash> GetEntitySkipTake(int skip, int take, string org, string? databaseName)
        {
            var sql = @$"{sqlStrStart.Replace("FROM VPayableCash c", $"FROM {DbQualifier.Qualify(databaseName, "VPayableCash")} c")}
                        CROSS APPLY STRING_SPLIT(c.Owner, ',') owners
                        WHERE (Author IN @orgList or owners.value IN @orgList)
                        ORDER BY Id DESC
                        OFFSET @skip ROWS
                        FETCH NEXT @take ROWS ONLY";

            string[] orgList = org.Split(',');

            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return db.Query<VPaymentCash>(sql, new { skip, take, orgList }).ToList();
            }
        }

        public IEnumerable<VPaymentCash> GetEntitySkipTake(int skip, int take, string queryWhere, string[] orgList)
        {
            var sql = @$"SELECT * FROM VPayableCash c
                        CROSS APPLY STRING_SPLIT(c.Owner, ',') owners
                        WHERE (Author IN @orgList or owners.value IN @orgList)
                            {queryWhere}
                        ORDER BY Id DESC
                        OFFSET @skip ROWS
                        FETCH NEXT @take ROWS ONLY";           

            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return db.Query<VPaymentCash>(sql, new { skip, take, orgList }).ToList();
            }
        }

        public IEnumerable<VPaymentCash> GetEntitySkipTake(int skip, int take, string queryWhere, string[] orgList, string? databaseName)
        {
            var sql = @$"{sqlStrStart.Replace("FROM VPayableCash c", $"FROM {DbQualifier.Qualify(databaseName, "VPayableCash")} c")}
                        CROSS APPLY STRING_SPLIT(c.Owner, ',') owners
                        WHERE (Author IN @orgList or owners.value IN @orgList)
                            {queryWhere}
                        ORDER BY Id DESC
                        OFFSET @skip ROWS
                        FETCH NEXT @take ROWS ONLY";           

            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return db.Query<VPaymentCash>(sql, new { skip, take, orgList }).ToList();
            }
        }
    }
}

