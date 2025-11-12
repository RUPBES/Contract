using Dapper;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.KDO;
using Microsoft.Data.SqlClient;
using System.Data;
using DatabaseLayer.RepositoriesDapper.Sql;

namespace DatabaseLayer.RepositoriesDapper.Repo
{
	public class ActDpRepository : IReadonlyRepoDapper<Act>
	{
		private readonly string? _connectionString = null;
		private readonly string sqlStrStart = @"SELECT * FROM Act c";

		public ActDpRepository(string connectionString)
		{
			_connectionString = connectionString;
		}

		public int Count()
		{
			using (IDbConnection db = new SqlConnection(_connectionString))
			{
				return db.Query<int>("SELECT COUNT(Id) FROM Act").FirstOrDefault();
			}
		}

		public int Count(string? databaseName)
		{
			string obj = DbQualifier.Qualify(databaseName, "Act");
			using (IDbConnection db = new SqlConnection(_connectionString))
			{
				return db.Query<int>($"SELECT COUNT(Id) FROM {obj}").FirstOrDefault();
			}
		}

		public IEnumerable<Act> Find(string predicate)
		{
			if (string.IsNullOrEmpty(predicate))
			{
				return Array.Empty<Act>();
			}

			using (IDbConnection db = new SqlConnection(_connectionString))
			{
				return db.Query<Act>($"{sqlStrStart} {predicate}").ToList();
			}
		}

		public IEnumerable<Act> Find(string predicate, string? databaseName)
		{
			if (string.IsNullOrEmpty(predicate))
			{
				return Array.Empty<Act>();
			}

			string sqlStart = sqlStrStart.Replace("FROM Act c", $"FROM {DbQualifier.Qualify(databaseName, "Act")} c");
			using (IDbConnection db = new SqlConnection(_connectionString))
			{
				return db.Query<Act>($"{sqlStart} {predicate}").ToList();
			}
		}

		public IEnumerable<Act> Find(string predicate, string[] orgList)
		{
			if (string.IsNullOrEmpty(predicate))
			{
				return Array.Empty<Act>();
			}

			using (IDbConnection db = new SqlConnection(_connectionString))
			{
				var sql = @$"SELECT c.* FROM Act c
						LEFT JOIN Contract cc ON c.ContractId = cc.Id
						CROSS APPLY STRING_SPLIT(cc.Owner, ',') owners
						WHERE (cc.Author IN @orgList or owners.value IN @orgList) {predicate}";
				return db.Query<Act>(sql, new { orgList }).ToList();
			}
		}

		public IEnumerable<Act> Find(string predicate, string[] orgList, string? databaseName)
		{
			if (string.IsNullOrEmpty(predicate))
			{
				return Array.Empty<Act>();
			}

			string actObj = DbQualifier.Qualify(databaseName, "Act");
			string contractObj = DbQualifier.Qualify(databaseName, "Contract");
			var sql = @$"SELECT c.* FROM {actObj} c
					LEFT JOIN {contractObj} cc ON c.ContractId = cc.Id
					CROSS APPLY STRING_SPLIT(cc.Owner, ',') owners
					WHERE (cc.Author IN @orgList or owners.value IN @orgList) {predicate}";

			using (IDbConnection db = new SqlConnection(_connectionString))
			{
				return db.Query<Act>(sql, new { orgList }).ToList();
			}
		}

		public IEnumerable<Act> GetAll()
		{
			using (IDbConnection db = new SqlConnection(_connectionString))
			{
				return db.Query<Act>(sqlStrStart).ToList();
			}
		}

		public IEnumerable<Act> GetAll(string? databaseName)
		{
			string sqlStart = sqlStrStart.Replace("FROM Act c", $"FROM {DbQualifier.Qualify(databaseName, "Act")} c");
			using (IDbConnection db = new SqlConnection(_connectionString))
			{
				return db.Query<Act>(sqlStart).ToList();
			}
		}

		public Act GetById(int id)
		{
			if (id > 0)
			{
				using (IDbConnection db = new SqlConnection(_connectionString))
				{
					return db.Query<Act>(@$"{sqlStrStart} WHERE c.Id = @id", new { id }).FirstOrDefault();
				}
			}
			return null;
		}

		public Act GetById(int id, string? databaseName)
		{
			if (id > 0)
			{
				string sqlStart = sqlStrStart.Replace("FROM Act c", $"FROM {DbQualifier.Qualify(databaseName, "Act")} c");
				using (IDbConnection db = new SqlConnection(_connectionString))
				{
					return db.Query<Act>(@$"{sqlStart} WHERE c.Id = @id", new { id }).FirstOrDefault();
				}
			}
			return null;
		}

		public IEnumerable<Act> GetEntitySkipTake(int skip, int take, string organizationName)
		{
			var sql = @$"SELECT c.* FROM Act c
					LEFT JOIN Contract cc ON c.ContractId = cc.Id
					CROSS APPLY STRING_SPLIT(cc.Owner, ',') owners
					WHERE (cc.Author IN @orgList or owners.value IN @orgList)
					ORDER BY c.Id DESC
					OFFSET @skip ROWS
					FETCH NEXT @take ROWS ONLY";

			string[] orgList = organizationName.Split(',');

			using (IDbConnection db = new SqlConnection(_connectionString))
			{
				return db.Query<Act>(sql, new { skip, take, orgList }).ToList();
			}
		}

		public IEnumerable<Act> GetEntitySkipTake(int skip, int take, string organizationName, string? databaseName)
		{
			var sql = @$"SELECT c.* FROM {DbQualifier.Qualify(databaseName, "Act")} c
					LEFT JOIN {DbQualifier.Qualify(databaseName, "Contract")} cc ON c.ContractId = cc.Id
					CROSS APPLY STRING_SPLIT(cc.Owner, ',') owners
					WHERE (cc.Author IN @orgList or owners.value IN @orgList)
					ORDER BY c.Id DESC
					OFFSET @skip ROWS
					FETCH NEXT @take ROWS ONLY";

			string[] orgList = organizationName.Split(',');

			using (IDbConnection db = new SqlConnection(_connectionString))
			{
				return db.Query<Act>(sql, new { skip, take, orgList }).ToList();
			}
		}
	}
}


