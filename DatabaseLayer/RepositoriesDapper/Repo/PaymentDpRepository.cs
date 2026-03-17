using Dapper;
using DatabaseLayer.Models.KDO;
using Microsoft.Data.SqlClient;
using DatabaseLayer.RepositoriesDapper.Sql;
using System.Data;
using DatabaseLayer.Interfaces.Dapper;

namespace DatabaseLayer.RepositoriesDapper.Repo
{
    public class PaymentDpRepository : IReadonlyPaymentDapperRepo
    {
        string _connectionString = null;
        public PaymentDpRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public int Count(string[] orgList, string? databaseName)
        {
            string vTable = DbQualifier.Qualify(databaseName, "VPayableCash");

            if (orgList is null || orgList?.Length == 0)
            {
                using (IDbConnection db = new SqlConnection(_connectionString))
                {
                    return db.Query<int>($"SELECT COUNT(Id) FROM {vTable}").FirstOrDefault();
                }
            }

            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return db.Query<int>(@$"SELECT distinct COUNT(Id) FROM {vTable} c									
									CROSS APPLY STRING_SPLIT(c.Owner, ',') owners
									WHERE (c.Author IN @orgList or owners.value IN @orgList)", new { orgList }).FirstOrDefault();
            }
        }

        public IEnumerable<VPaymentCash> Find(string predicate, string[] orgList, string? databaseName)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                string vTable = DbQualifier.Qualify(databaseName, "VPayableCash");
                return db.Query<VPaymentCash>($"SELECT distinct c.* FROM {vTable} c CROSS APPLY STRING_SPLIT(c.Owner, ',') owners WHERE (Author IN @orgList or owners.value IN @orgList) {predicate}", new { orgList }).ToList();
            }
        }

        public IEnumerable<VPaymentCash> GetAll(string? databaseName)
        {
            string vTable = DbQualifier.Qualify(databaseName, "VPayableCash");
            
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return db.Query<VPaymentCash>($"SELECT c.* FROM {vTable} c").ToList();
            }
        }

        public VPaymentCash GetById(int id, string? databaseName)
        {
            if (id > 0)
            {
                string vTable = DbQualifier.Qualify(databaseName, "VPayableCash");
                using (IDbConnection db = new SqlConnection(_connectionString))
                {
                    return db.QueryFirstOrDefault<VPaymentCash>(@$"SELECT c.* FROM {vTable} c where c.Id = @id", new { id });
                }
            }
            return null;
        }

        public IEnumerable<VPaymentCash> GetEntitySkipTake(int skip, int take, string org, string? databaseName)
        {
            string vTable = DbQualifier.Qualify(databaseName, "VPayableCash");

            var sql = @$"SELECT distinct c.* FROM {vTable} c
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
                
        public IEnumerable<VPaymentCash> GetEntitySkipTake(int skip, int take, string queryWhere, string[] orgList, string? databaseName)
        {
            string vTable = DbQualifier.Qualify(databaseName, "VPayableCash");
            
            var sql = @$"SELECT distinct c.* FROM {vTable} c
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


        /// <summary>
        /// Фильтрация и сортировка данных
        /// </summary>
        /// <param name="skip">Количество записей которые пропускаем</param>
        /// <param name="take">Количество записей которые забираем</param>
        /// <param name="org">Строка с названиями организаций, данные которых видны для пользователя</param>
        /// <param name="query">Строка с условием запроса ( "начинаться должна с "and ", после чего условие помещается в скобки "(....)")</param>
        /// <param name="databaseName">название БД, если необходимо взять данные, например из архивной БД</param>
        /// <returns></returns>
        public (IEnumerable<VPaymentCash>, int) Filter(int skip, int take, string org, string? query, string? orderBy)
        {
            string[] orgList = org.Split(',');
            var sql = @$"
                        SELECT distinct COUNT(DISTINCT c.Id) FROM VPayableCash c 
                        CROSS APPLY STRING_SPLIT(c.Owner, ',') owners
                        WHERE (Author IN @orgList or owners.value IN @orgList)
                        {query};

                        SELECT distinct c.* FROM VPayableCash c
                        CROSS APPLY STRING_SPLIT(c.Owner, ',') owners
                        WHERE (Author IN @orgList or owners.value IN @orgList)
                        {query} {orderBy}
                        OFFSET @skip ROWS
                        FETCH NEXT @take ROWS ONLY;";

            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                var multi = db.QueryMultiple(sql, new { skip, take, orgList });
                var totalCount = multi.ReadSingle<int>();
                var contracts = multi.Read<VPaymentCash>().ToList();
                return (contracts, totalCount);
            }
        }
    }
}