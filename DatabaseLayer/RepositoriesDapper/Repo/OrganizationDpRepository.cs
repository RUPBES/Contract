using Dapper;
using DatabaseLayer.Interfaces.Dapper;
using DatabaseLayer.Models.EXTRA;
using DatabaseLayer.Models.KDO;
using DatabaseLayer.RepositoriesDapper.Sql;
using Microsoft.Data.SqlClient;
using System.Data;

namespace DatabaseLayer.RepositoriesDapper.Repo
{
    public class OrganizationDpRepository : IReadonlyOrganizationDapperRepo
    {
        string _connectionString = null;
        public OrganizationDpRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IEnumerable<Organization> Find(string predicate, string[] orgList, string? databaseName)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                string vTable = DbQualifier.Qualify(databaseName, "Organization");
                return db.Query<Organization>($"SELECT distinct c.* FROM {vTable} c where {predicate}").ToList();
            }
        }

        public IEnumerable<Organization> GetAll(string? databaseName)
        {
            string vTable = DbQualifier.Qualify(databaseName, "Organization");

            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return db.Query<Organization>($"SELECT c.* FROM {vTable} c").ToList();
            }
        }

        public Organization GetById(int id, string? databaseName)
        {
            if (id > 0)
            {
                string vTable = DbQualifier.Qualify(databaseName, "Organization");
                using (IDbConnection db = new SqlConnection(_connectionString))
                {
                    return db.QueryFirstOrDefault<Organization>(@$"SELECT c.* FROM {vTable} c where c.Id = @id", new { id });
                }
            }
            return null;
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
        public (IEnumerable<OrganizationRecord>, int) Filter(int skip, int take, string? query, string? orderBy, string? databaseName)
        {
            string table = DbQualifier.Qualify(databaseName, "Organization");
            string queryJoinAddressTableOrNot = string.Empty;
            string tableAdress = DbQualifier.Qualify(databaseName, "Address");
            if (query?.Contains("FullAddress LIKE") == true)
            {
                queryJoinAddressTableOrNot = $" LEFT JOIN {tableAdress} la ON o.Id = la.OrganizationId {query}";
            }
            else
            {
                queryJoinAddressTableOrNot = query ?? "";
            }

            var sql = @$"
                        SELECT distinct COUNT(o.Id) FROM {table} o                         
                        {queryJoinAddressTableOrNot};

                        WITH LastAddress AS (
                            SELECT OrganizationId,FullAddress, FullAddressFact, PostIndex, SiteAddress FROM (
                                SELECT 
                                    OrganizationId, FullAddress, FullAddressFact, PostIndex, SiteAddress,
                                    ROW_NUMBER() OVER (
                                        PARTITION BY OrganizationId 
                                        ORDER BY Id DESC
                                    ) AS rn
                                FROM {tableAdress}) a
                            WHERE rn = 1
                        ),
                        PhoneList AS (
                            SELECT OrganizationId, STRING_AGG(Number, ', ') AS PhoneNumbers
                            FROM {DbQualifier.Qualify(databaseName, "Phone")}
                            GROUP BY OrganizationId
                        )
                        SELECT 
                            o.Id,o.Name,o.Email,o.PaymentAccount,la.FullAddress,la.FullAddressFact, la.PostIndex, la.SiteAddress, ISNULL(pl.PhoneNumbers, '') AS PhoneNumbers
                        FROM {table} o
                            LEFT JOIN LastAddress la ON o.Id = la.OrganizationId
                            LEFT JOIN PhoneList pl ON o.Id = pl.OrganizationId
                        {query} 
                        {orderBy}
                        OFFSET @skip ROWS
                        FETCH NEXT @take ROWS ONLY;";

            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                var multi = db.QueryMultiple(sql, new { skip, take });
                var totalCount = multi.ReadSingle<int>();
                var contracts = multi.Read<OrganizationRecord>().ToList();
                return (contracts, totalCount);
            }
        }
    }
}