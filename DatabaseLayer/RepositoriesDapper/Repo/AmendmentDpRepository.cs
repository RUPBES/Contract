using Dapper;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.KDO;
using Microsoft.Data.SqlClient;
using System.Data;
using DatabaseLayer.RepositoriesDapper.Sql;

namespace DatabaseLayer.RepositoriesDapper.Repo
{
	public class AmendmentDpRepository : IReadonlyRepoDapper<Amendment>
	{
		private readonly string? _connectionString = null;
		private readonly string sqlStrStart = @"SELECT * FROM Amendment c";

		public AmendmentDpRepository(string connectionString)
		{
			_connectionString = connectionString;
		}

		public int Count()
		{
			using (IDbConnection db = new SqlConnection(_connectionString))
			{
				return db.Query<int>("SELECT COUNT(Id) FROM Amendment").FirstOrDefault();
			}
		}

		public int Count(string? databaseName)
		{
			string obj = DbQualifier.Qualify(databaseName, "Amendment");
			using (IDbConnection db = new SqlConnection(_connectionString))
			{
				return db.Query<int>($"SELECT COUNT(Id) FROM {obj}").FirstOrDefault();
			}
		}

		public IEnumerable<Amendment> Find(string predicate)
		{
			if (string.IsNullOrEmpty(predicate))
			{
				return Array.Empty<Amendment>();
			}

			using (IDbConnection db = new SqlConnection(_connectionString))
			{
				return db.Query<Amendment>($"{sqlStrStart} {predicate}").ToList();
			}
		}

		public IEnumerable<Amendment> Find(string predicate, string? databaseName)
		{
			if (string.IsNullOrEmpty(predicate))
			{
				return Array.Empty<Amendment>();
			}

			string sqlStart = sqlStrStart.Replace("FROM Amendment c", $"FROM {DbQualifier.Qualify(databaseName, "Amendment")} c");
			using (IDbConnection db = new SqlConnection(_connectionString))
			{
				return db.Query<Amendment>($"{sqlStart} {predicate}").ToList();
			}
		}

		public IEnumerable<Amendment> Find(string predicate, string[] orgList)
		{
			if (string.IsNullOrEmpty(predicate))
			{
				return Array.Empty<Amendment>();
			}

			using (IDbConnection db = new SqlConnection(_connectionString))
			{
				var sql = @$"SELECT c.* FROM Amendment c
						LEFT JOIN Contract cc ON c.ContractId = cc.Id
						CROSS APPLY STRING_SPLIT(cc.Owner, ',') owners
						WHERE (cc.Author IN @orgList or owners.value IN @orgList) {predicate}";
				return db.Query<Amendment>(sql, new { orgList }).ToList();
			}
		}

		public IEnumerable<Amendment> Find(string predicate, string[] orgList, string? databaseName)
		{
			if (string.IsNullOrEmpty(predicate))
			{
				return Array.Empty<Amendment>();
			}

			string amendObj = DbQualifier.Qualify(databaseName, "Amendment");
			string contractObj = DbQualifier.Qualify(databaseName, "Contract");
			var sql = @$"SELECT c.* FROM {amendObj} c
					LEFT JOIN {contractObj} cc ON c.ContractId = cc.Id
					CROSS APPLY STRING_SPLIT(cc.Owner, ',') owners
					WHERE (cc.Author IN @orgList or owners.value IN @orgList) {predicate}";

			using (IDbConnection db = new SqlConnection(_connectionString))
			{
				return db.Query<Amendment>(sql, new { orgList }).ToList();
			}
		}

		public IEnumerable<Amendment> GetAll()
		{
			using (IDbConnection db = new SqlConnection(_connectionString))
			{
				return db.Query<Amendment>(sqlStrStart).ToList();
			}
		}

		public IEnumerable<Amendment> GetAll(string? databaseName)
		{
			string sqlStart = sqlStrStart.Replace("FROM Amendment c", $"FROM {DbQualifier.Qualify(databaseName, "Amendment")} c");
			using (IDbConnection db = new SqlConnection(_connectionString))
			{
				return db.Query<Amendment>(sqlStart).ToList();
			}
		}

		public Amendment GetById(int id)
		{
			if (id > 0)
			{
				using (IDbConnection db = new SqlConnection(_connectionString))
				{
					return db.Query<Amendment>(@$"{sqlStrStart} WHERE c.Id = @id", new { id }).FirstOrDefault();
				}
			}
			return null;
		}

		public Amendment GetById(int id, string? databaseName)
		{
			if (id > 0)
			{
				string sqlStart = sqlStrStart.Replace("FROM Amendment c", $"FROM {DbQualifier.Qualify(databaseName, "Amendment")} c");
				using (IDbConnection db = new SqlConnection(_connectionString))
				{
					return db.Query<Amendment>(@$"{sqlStart} WHERE c.Id = @id", new { id }).FirstOrDefault();
				}
			}
			return null;
		}

		public IEnumerable<Amendment> GetEntitySkipTake(int skip, int take, string organizationName)
		{
			var sql = @$"SELECT c.* FROM Amendment c
					LEFT JOIN Contract cc ON c.ContractId = cc.Id
					CROSS APPLY STRING_SPLIT(cc.Owner, ',') owners
					WHERE (cc.Author IN @orgList or owners.value IN @orgList)
					ORDER BY c.Id DESC
					OFFSET @skip ROWS
					FETCH NEXT @take ROWS ONLY";

			string[] orgList = organizationName.Split(',');

			using (IDbConnection db = new SqlConnection(_connectionString))
			{
				return db.Query<Amendment>(sql, new { skip, take, orgList }).ToList();
			}
		}

		public IEnumerable<Amendment> GetEntitySkipTake(int skip, int take, string organizationName, string? databaseName)
		{
			var sql = @$"SELECT c.* FROM {DbQualifier.Qualify(databaseName, "Amendment")} c
					LEFT JOIN {DbQualifier.Qualify(databaseName, "Contract")} cc ON c.ContractId = cc.Id
					CROSS APPLY STRING_SPLIT(cc.Owner, ',') owners
					WHERE (cc.Author IN @orgList or owners.value IN @orgList)
					ORDER BY c.Id DESC
					OFFSET @skip ROWS
					FETCH NEXT @take ROWS ONLY";

			string[] orgList = organizationName.Split(',');

			using (IDbConnection db = new SqlConnection(_connectionString))
			{
				return db.Query<Amendment>(sql, new { skip, take, orgList }).ToList();
			}
		}
	}
}


