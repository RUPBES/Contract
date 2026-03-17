using Dapper;
using DatabaseLayer.Interfaces.Dapper;
using DatabaseLayer.Models.KDO;
using DatabaseLayer.RepositoriesDapper.Sql;
using Microsoft.Data.SqlClient;
using System.Data;

namespace DatabaseLayer.RepositoriesDapper.Repo;

public class ScopeWorkDpRepository : IReadonlyRepoDapper<ScopeWork>
{
    private readonly string? _connectionString = null;

    public ScopeWorkDpRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public int Count(string[] orgList, string? databaseName)
    {
        string table = DbQualifier.Qualify(databaseName, "ScopeWork");

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


    public IEnumerable<ScopeWork> Find(string predicate, string[] orgList, string? databaseName)
    {
        if (string.IsNullOrEmpty(predicate))
        {
            return Array.Empty<ScopeWork>();
        }

        string scope = DbQualifier.Qualify(databaseName, "ScopeWork");
        string joinContract = DbQualifier.Qualify(databaseName, "Contract");
        var sql = @$"SELECT distinct c.* FROM {scope} c
					LEFT JOIN {joinContract} cc ON c.ContractId = cc.Id
					CROSS APPLY STRING_SPLIT(cc.Owner, ',') owners
					WHERE (cc.Author IN @orgList or owners.value IN @orgList) {predicate}";

        using (IDbConnection db = new SqlConnection(_connectionString))
        {
            return db.Query<ScopeWork>(sql, new { orgList }).ToList();
        }
    }

    public IEnumerable<ScopeWork> GetAll(string? databaseName)
    {
        string scope = DbQualifier.Qualify(databaseName, "ScopeWork");

        using (IDbConnection db = new SqlConnection(_connectionString))
        {
            return db.Query<ScopeWork>(@$"SELECT c.* FROM {scope} c").ToList();
        }
    }

    public ScopeWork GetById(int id, string? databaseName)
    {
        if (id > 0)
        {
            string scope = DbQualifier.Qualify(databaseName, "ScopeWork");

            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return db.Query<ScopeWork>(@$"SELECT c.* FROM {scope} c WHERE c.Id = @id", new { id }).FirstOrDefault();
            }
        }
        return null;
    }

    public IEnumerable<ScopeWork> GetEntitySkipTake(int skip, int take, string organizationName, string? databaseName)
    {
        string table = DbQualifier.Qualify(databaseName, "ScopeWork");
        string joinContract = DbQualifier.Qualify(databaseName, "Contract");

        var sql = @$"SELECT c.* FROM {table} c
					LEFT JOIN {joinContract} cc ON c.ContractId = cc.Id
					CROSS APPLY STRING_SPLIT(cc.Owner, ',') owners
					WHERE (cc.Author IN @orgList or owners.value IN @orgList)
					ORDER BY c.Id DESC
					OFFSET @skip ROWS
					FETCH NEXT @take ROWS ONLY";

        string[] orgList = organizationName.Split(',');

        using (IDbConnection db = new SqlConnection(_connectionString))
        {
            return db.Query<ScopeWork>(sql, new { skip, take, orgList }).ToList();
        }
    }
}