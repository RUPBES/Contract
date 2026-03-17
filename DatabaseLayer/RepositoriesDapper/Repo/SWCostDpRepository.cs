using Dapper;
using DatabaseLayer.Interfaces.Dapper;
using DatabaseLayer.Models.KDO;
using DatabaseLayer.RepositoriesDapper.Sql;
using Microsoft.Data.SqlClient;
using System.Data;

namespace DatabaseLayer.RepositoriesDapper.Repo
{
	public class SWCostDpRepository : IReadonlyRepoDapper<SWCost>
	{
		private readonly string? _connectionString = null;
		public SWCostDpRepository(string connectionString)
		{
			_connectionString = connectionString;
		}

		public int Count(string[] orgList, string? databaseName)
		{
            if (orgList is null || orgList?.Length == 0)
            {
                using (IDbConnection db = new SqlConnection(_connectionString))
                {
                    return db.Query<int>($"SELECT COUNT(Id) FROM { DbQualifier.Qualify(databaseName, "SWCost") }").FirstOrDefault();
                }
            }

            using (IDbConnection db = new SqlConnection(_connectionString))
			{
				return db.Query<int>(@$"SELECT distinct COUNT(Id) FROM {DbQualifier.Qualify(databaseName, "SWCost")} c
															LEFT JOIN {DbQualifier.Qualify(databaseName, "ScopeWork")} sw ON c.ScopeWorkId = sw.Id
															LEFT JOIN {DbQualifier.Qualify(databaseName, "Contract")} cc ON sw.ContractId = cc.Id
															CROSS APPLY STRING_SPLIT(cc.Owner, ',') owners
															WHERE (cc.Author IN @orgList or owners.value IN @orgList)", new { orgList })
						 .FirstOrDefault();
			}
		}

		public IEnumerable<SWCost> Find(string predicate, string[] orgList, string? databaseName)
		{
			if (string.IsNullOrEmpty(predicate)) 
				return Array.Empty<SWCost>();
			
			using (IDbConnection db = new SqlConnection(_connectionString))
			{
				return db.Query<SWCost>(@$"SELECT distinct c.* FROM {DbQualifier.Qualify(databaseName, "SWCost")} c
														LEFT JOIN {DbQualifier.Qualify(databaseName, "ScopeWork")} sw ON c.ScopeWorkId = sw.Id
														LEFT JOIN {DbQualifier.Qualify(databaseName, "Contract")} cc ON sw.ContractId = cc.Id
														CROSS APPLY STRING_SPLIT(cc.Owner, ',') owners
														WHERE (cc.Author IN @orgList or owners.value IN @orgList) {predicate}", new { orgList })
						 .ToList();
			}
		}

		public IEnumerable<SWCost> GetAll(string? databaseName)
		{			
			using (IDbConnection db = new SqlConnection(_connectionString))
			{
				return db.Query<SWCost>($"SELECT c.* FROM {DbQualifier.Qualify(databaseName, "SWCost")}").ToList();
			}
		}

		public SWCost GetById(int id, string? databaseName)
		{
			if (id > 0)
			{				
				using (IDbConnection db = new SqlConnection(_connectionString))
				{
					return db.Query<SWCost>(@$"SELECT c.* FROM {DbQualifier.Qualify(databaseName, "SWCost")} c WHERE c.Id = @id", new { id }).FirstOrDefault();
				}
			}
			return null;
		}

		public IEnumerable<SWCost> GetEntitySkipTake(int skip, int take, string organizationName, string? databaseName)
		{
			var sql = @$"SELECT c.* FROM {DbQualifier.Qualify(databaseName, "SWCost")} c
					LEFT JOIN {DbQualifier.Qualify(databaseName, "ScopeWork")} sw ON c.ScopeWorkId = sw.Id
					LEFT JOIN {DbQualifier.Qualify(databaseName, "Contract")} cc ON sw.ContractId = cc.Id
					CROSS APPLY STRING_SPLIT(cc.Owner, ',') owners
					WHERE (cc.Author IN @orgList or owners.value IN @orgList)
					ORDER BY c.Id DESC
					OFFSET @skip ROWS
					FETCH NEXT @take ROWS ONLY";

			string[] orgList = organizationName.Split(',');

			using (IDbConnection db = new SqlConnection(_connectionString))
			{
				return db.Query<SWCost>(sql, new { skip, take, orgList }).ToList();
			}
		}
	}
}


