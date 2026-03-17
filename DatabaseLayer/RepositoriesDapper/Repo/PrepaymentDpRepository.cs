using Dapper;
using DatabaseLayer.Models.KDO;
using Microsoft.Data.SqlClient;
using System.Data;
using DatabaseLayer.RepositoriesDapper.Sql;
using DatabaseLayer.Interfaces.Dapper;

namespace DatabaseLayer.RepositoriesDapper.Repo;

public class PrepaymentDpRepository : IReadonlyRepoDapper<Prepayment>
{
    private readonly string? _connectionString = null;
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
}