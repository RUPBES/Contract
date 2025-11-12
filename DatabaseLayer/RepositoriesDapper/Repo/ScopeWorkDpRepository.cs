using Dapper;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.KDO;
using Microsoft.Data.SqlClient;
using System.Data;
using DatabaseLayer.RepositoriesDapper.Sql;

namespace DatabaseLayer.RepositoriesDapper.Repo
{
	public class ScopeWorkDpRepository : IReadonlyRepoDapper<ScopeWork>
	{
		private readonly string? _connectionString = null;
		private readonly string sqlStrStart = @"SELECT * FROM ScopeWork c";

		public ScopeWorkDpRepository(string connectionString)
		{
			_connectionString = connectionString;
		}

		public int Count()
		{
			using (IDbConnection db = new SqlConnection(_connectionString))
			{
				return db.Query<int>("SELECT COUNT(Id) FROM ScopeWork").FirstOrDefault();
			}
		}

		public int Count(string? databaseName)
		{
			string obj = DbQualifier.Qualify(databaseName, "ScopeWork");
			using (IDbConnection db = new SqlConnection(_connectionString))
			{
				return db.Query<int>($"SELECT COUNT(Id) FROM {obj}").FirstOrDefault();
			}
		}

		public IEnumerable<ScopeWork> Find(string predicate)
		{
			if (string.IsNullOrEmpty(predicate))
			{
				return Array.Empty<ScopeWork>();
			}

			using (IDbConnection db = new SqlConnection(_connectionString))
			{
				return db.Query<ScopeWork>($"{sqlStrStart} {predicate}").ToList();
			}
		}

		public IEnumerable<ScopeWork> Find(string predicate, string? databaseName)
		{
			if (string.IsNullOrEmpty(predicate))
			{
				return Array.Empty<ScopeWork>();
			}

			string sqlStart = sqlStrStart.Replace("FROM ScopeWork c", $"FROM {DbQualifier.Qualify(databaseName, "ScopeWork")} c");
			using (IDbConnection db = new SqlConnection(_connectionString))
			{
				return db.Query<ScopeWork>($"{sqlStart} {predicate}").ToList();
			}
		}

		public IEnumerable<ScopeWork> Find(string predicate, string[] orgList)
		{
			if (string.IsNullOrEmpty(predicate))
			{
				return Array.Empty<ScopeWork>();
			}

			using (IDbConnection db = new SqlConnection(_connectionString))
			{
				var sql = @$"SELECT c.* FROM ScopeWork c
						LEFT JOIN Contract cc ON c.ContractId = cc.Id
						CROSS APPLY STRING_SPLIT(cc.Owner, ',') owners
						WHERE (cc.Author IN @orgList or owners.value IN @orgList) {predicate}";
				return db.Query<ScopeWork>(sql, new { orgList }).ToList();
			}
		}

		public IEnumerable<ScopeWork> Find(string predicate, string[] orgList, string? databaseName)
		{
			if (string.IsNullOrEmpty(predicate))
			{
				return Array.Empty<ScopeWork>();
			}

			string scopeObj = DbQualifier.Qualify(databaseName, "ScopeWork");
			string contractObj = DbQualifier.Qualify(databaseName, "Contract");
			var sql = @$"SELECT c.* FROM {scopeObj} c
					LEFT JOIN {contractObj} cc ON c.ContractId = cc.Id
					CROSS APPLY STRING_SPLIT(cc.Owner, ',') owners
					WHERE (cc.Author IN @orgList or owners.value IN @orgList) {predicate}";

			using (IDbConnection db = new SqlConnection(_connectionString))
			{
				return db.Query<ScopeWork>(sql, new { orgList }).ToList();
			}
		}

		public IEnumerable<ScopeWork> GetAll()
		{
			using (IDbConnection db = new SqlConnection(_connectionString))
			{
				return db.Query<ScopeWork>(sqlStrStart).ToList();
			}
		}

		public IEnumerable<ScopeWork> GetAll(string? databaseName)
		{
			string sqlStart = sqlStrStart.Replace("FROM ScopeWork c", $"FROM {DbQualifier.Qualify(databaseName, "ScopeWork")} c");
			using (IDbConnection db = new SqlConnection(_connectionString))
			{
				return db.Query<ScopeWork>(sqlStart).ToList();
			}
		}

		public ScopeWork GetById(int id)
		{
			if (id > 0)
			{
				using (IDbConnection db = new SqlConnection(_connectionString))
				{
					return db.Query<ScopeWork>(@$"{sqlStrStart} WHERE c.Id = @id", new { id }).FirstOrDefault();
				}
			}
			return null;
		}

		public ScopeWork GetById(int id, string? databaseName)
		{
			if (id > 0)
			{
				string sqlStart = sqlStrStart.Replace("FROM ScopeWork c", $"FROM {DbQualifier.Qualify(databaseName, "ScopeWork")} c");
				using (IDbConnection db = new SqlConnection(_connectionString))
				{
					return db.Query<ScopeWork>(@$"{sqlStart} WHERE c.Id = @id", new { id }).FirstOrDefault();
				}
			}
			return null;
		}

		public IEnumerable<ScopeWork> GetEntitySkipTake(int skip, int take, string organizationName)
		{
			var sql = @$"SELECT c.* FROM ScopeWork c
					LEFT JOIN Contract cc ON c.ContractId = cc.Id
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

		public IEnumerable<ScopeWork> GetEntitySkipTake(int skip, int take, string organizationName, string? databaseName)
		{
			var sql = @$"SELECT c.* FROM {DbQualifier.Qualify(databaseName, "ScopeWork")} c
					LEFT JOIN {DbQualifier.Qualify(databaseName, "Contract")} cc ON c.ContractId = cc.Id
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
}


