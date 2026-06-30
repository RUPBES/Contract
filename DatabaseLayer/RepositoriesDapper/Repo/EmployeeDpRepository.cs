using Dapper;
using DatabaseLayer.Interfaces.Dapper;
using DatabaseLayer.Models.EXTRA;
using DatabaseLayer.Models.KDO;
using DatabaseLayer.RepositoriesDapper.Sql;
using Microsoft.Data.SqlClient;
using System.Data;

namespace DatabaseLayer.RepositoriesDapper.Repo;

public class EmployeeDpRepository : IReadonlyEmployeeDapperRepo
{
    string _connectionString = null;
    public EmployeeDpRepository(string connectionString)
    {
        _connectionString = connectionString;
    }
    public IEnumerable<Employee> Find(string predicate, string[] orgList, string? databaseName = null)
    {
        return Array.Empty<Employee>();
    }

    public IEnumerable<Employee> GetAll(string? databaseName = null)
    {
        return Array.Empty<Employee>();
    }

    public Employee GetById(int id, string? databaseName = null)
    {
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
    public (IEnumerable<EmployeeRecord>, int) Filter(int skip, int take, string org, string? query, string? orderBy, string? databaseName)
    {
        string[] orgList = org.Split(',');
        string table = DbQualifier.Qualify(databaseName, "Employee");       
        var sql = @$"
            SELECT COUNT(distinct e.Id) FROM {table} e 
                WHERE (e.Author IN @orgList OR e.Author is NULL)                 
                {query} {(!string.IsNullOrEmpty(databaseName)? "": "AND e.IsActive = 1")} ;

            WITH 
            PhoneList AS (
                SELECT distinct EmployeeId, STRING_AGG(Number, ', ') AS PhoneNumbers
                FROM {DbQualifier.Qualify(databaseName, "Phone")}
                GROUP BY EmployeeId
            )
            SELECT distinct e.Id,e.FullName,e.FIO,e.Email, e.Position, e.Author, ISNULL(pl.PhoneNumbers, '') AS PhoneNumbers
            FROM {table} e                            
            LEFT JOIN PhoneList pl ON e.Id = pl.EmployeeId                        
            WHERE (e.Author IN @orgList OR e.Author is NULL)                 
            {query} {(!string.IsNullOrEmpty(databaseName) ? "" : "AND e.IsActive = 1")}
            {orderBy}
            OFFSET @skip ROWS
            FETCH NEXT @take ROWS ONLY;";

        using (IDbConnection db = new SqlConnection(_connectionString))
        {
            var multi = db.QueryMultiple(sql, new { skip, take, orgList });
            var totalCount = multi.ReadSingle<int>();
            var contracts = multi.Read<EmployeeRecord>().ToList();
            return (contracts, totalCount);
        }
    }
}
