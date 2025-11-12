using Dapper;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.KDO;
using Microsoft.Data.SqlClient;
using System.Data;
using DatabaseLayer.RepositoriesDapper.Sql;

namespace DatabaseLayer.RepositoriesDapper.Repo
{
	public class SWCostDpRepository : IReadonlyRepoDapper<SWCost>
	{
		private readonly string? _connectionString = null;
		private readonly string sqlStrStart = @"SELECT * FROM SWCost c";

		public SWCostDpRepository(string connectionString)
		{
			_connectionString = connectionString;
		}

		public int Count()
		{
			using (IDbConnection db = new SqlConnection(_connectionString))
			{
				return db.Query<int>("SELECT COUNT(Id) FROM SWCost").FirstOrDefault();
			}
		}

		public int Count(string? databaseName)
		{
			string obj = DbQualifier.Qualify(databaseName, "SWCost");
			using (IDbConnection db = new SqlConnection(_connectionString))
			{
				return db.Query<int>($"SELECT COUNT(Id) FROM {obj}").FirstOrDefault();
			}
		}

		public IEnumerable<SWCost> Find(string predicate)
		{
			if (string.IsNullOrEmpty(predicate))
			{
				return Array.Empty<SWCost>();
			}

			using (IDbConnection db = new SqlConnection(_connectionString))
			{
				return db.Query<SWCost>($"{sqlStrStart} {predicate}").ToList();
			}
		}

		public IEnumerable<SWCost> Find(string predicate, string? databaseName)
		{
			if (string.IsNullOrEmpty(predicate))
			{
				return Array.Empty<SWCost>();
			}

			string sqlStart = sqlStrStart.Replace("FROM SWCost c", $"FROM {DbQualifier.Qualify(databaseName, "SWCost")} c");
			using (IDbConnection db = new SqlConnection(_connectionString))
			{
				return db.Query<SWCost>($"{sqlStart} {predicate}").ToList();
			}
		}

		public IEnumerable<SWCost> Find(string predicate, string[] orgList)
		{
			if (string.IsNullOrEmpty(predicate))
			{
				return Array.Empty<SWCost>();
			}

			using (IDbConnection db = new SqlConnection(_connectionString))
			{
				var sql = @$"SELECT c.* FROM SWCost c
						LEFT JOIN ScopeWork sw ON c.ScopeWorkId = sw.Id
						LEFT JOIN Contract cc ON sw.ContractId = cc.Id
						CROSS APPLY STRING_SPLIT(cc.Owner, ',') owners
						WHERE (cc.Author IN @orgList or owners.value IN @orgList) {predicate}";
				return db.Query<SWCost>(sql, new { orgList }).ToList();
			}
		}

		public IEnumerable<SWCost> Find(string predicate, string[] orgList, string? databaseName)
		{
			if (string.IsNullOrEmpty(predicate))
			{
				return Array.Empty<SWCost>();
			}

			string costObj = DbQualifier.Qualify(databaseName, "SWCost");
			string scopeObj = DbQualifier.Qualify(databaseName, "ScopeWork");
			string contractObj = DbQualifier.Qualify(databaseName, "Contract");
			var sql = @$"SELECT c.* FROM {costObj} c
					LEFT JOIN {scopeObj} sw ON c.ScopeWorkId = sw.Id
					LEFT JOIN {contractObj} cc ON sw.ContractId = cc.Id
					CROSS APPLY STRING_SPLIT(cc.Owner, ',') owners
					WHERE (cc.Author IN @orgList or owners.value IN @orgList) {predicate}";

			using (IDbConnection db = new SqlConnection(_connectionString))
			{
				return db.Query<SWCost>(sql, new { orgList }).ToList();
			}
		}

		public IEnumerable<SWCost> GetAll()
		{
			using (IDbConnection db = new SqlConnection(_connectionString))
			{
				return db.Query<SWCost>(sqlStrStart).ToList();
			}
		}

		public IEnumerable<SWCost> GetAll(string? databaseName)
		{
			string sqlStart = sqlStrStart.Replace("FROM SWCost c", $"FROM {DbQualifier.Qualify(databaseName, "SWCost")} c");
			using (IDbConnection db = new SqlConnection(_connectionString))
			{
				return db.Query<SWCost>(sqlStart).ToList();
			}
		}

		public SWCost GetById(int id)
		{
			if (id > 0)
			{
				using (IDbConnection db = new SqlConnection(_connectionString))
				{
					return db.Query<SWCost>(@$"{sqlStrStart} WHERE c.Id = @id", new { id }).FirstOrDefault();
				}
			}
			return null;
		}

		public SWCost GetById(int id, string? databaseName)
		{
			if (id > 0)
			{
				string sqlStart = sqlStrStart.Replace("FROM SWCost c", $"FROM {DbQualifier.Qualify(databaseName, "SWCost")} c");
				using (IDbConnection db = new SqlConnection(_connectionString))
				{
					return db.Query<SWCost>(@$"{sqlStart} WHERE c.Id = @id", new { id }).FirstOrDefault();
				}
			}
			return null;
		}

		public IEnumerable<SWCost> GetEntitySkipTake(int skip, int take, string organizationName)
		{
			var sql = @$"SELECT c.* FROM SWCost c
					LEFT JOIN ScopeWork sw ON c.ScopeWorkId = sw.Id
					LEFT JOIN Contract cc ON sw.ContractId = cc.Id
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


