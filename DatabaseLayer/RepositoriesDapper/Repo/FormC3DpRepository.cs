using Dapper;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.KDO;
using Microsoft.Data.SqlClient;
using System.Data;
using DatabaseLayer.RepositoriesDapper.Sql;

namespace DatabaseLayer.RepositoriesDapper.Repo
{
	public class FormC3DpRepository : IReadonlyRepoDapper<FormC3a>
	{
		private readonly string? _connectionString = null;
		private readonly string sqlStrStart = @"SELECT * FROM FormC3a c";

		public FormC3DpRepository(string connectionString)
		{
			_connectionString = connectionString;
		}

		public int Count()
		{
			using (IDbConnection db = new SqlConnection(_connectionString))
			{
				return db.Query<int>("SELECT COUNT(Id) FROM FormC3a").FirstOrDefault();
			}
		}

		public int Count(string? databaseName)
		{
			string obj = DbQualifier.Qualify(databaseName, "FormC3a");
			using (IDbConnection db = new SqlConnection(_connectionString))
			{
				return db.Query<int>($"SELECT COUNT(Id) FROM {obj}").FirstOrDefault();
			}
		}

		public IEnumerable<FormC3a> Find(string predicate)
		{
			if (string.IsNullOrEmpty(predicate))
			{
				return Array.Empty<FormC3a>();
			}

			using (IDbConnection db = new SqlConnection(_connectionString))
			{
				return db.Query<FormC3a>($"{sqlStrStart} {predicate}").ToList();
			}
		}

		public IEnumerable<FormC3a> Find(string predicate, string? databaseName)
		{
			if (string.IsNullOrEmpty(predicate))
			{
				return Array.Empty<FormC3a>();
			}

			string sqlStart = sqlStrStart.Replace("FROM FormC3a c", $"FROM {DbQualifier.Qualify(databaseName, "FormC3a")} c");
			using (IDbConnection db = new SqlConnection(_connectionString))
			{
				return db.Query<FormC3a>($"{sqlStart} {predicate}").ToList();
			}
		}

		public IEnumerable<FormC3a> Find(string predicate, string[] orgList)
		{
			if (string.IsNullOrEmpty(predicate))
			{
				return Array.Empty<FormC3a>();
			}

			using (IDbConnection db = new SqlConnection(_connectionString))
			{
				var sql = @$"SELECT c.* FROM FormC3a c
						LEFT JOIN Contract cc ON c.ContractId = cc.Id
						CROSS APPLY STRING_SPLIT(cc.Owner, ',') owners
						WHERE (cc.Author IN @orgList or owners.value IN @orgList) {predicate}";
				return db.Query<FormC3a>(sql, new { orgList }).ToList();
			}
		}

		public IEnumerable<FormC3a> Find(string predicate, string[] orgList, string? databaseName)
		{
			if (string.IsNullOrEmpty(predicate))
			{
				return Array.Empty<FormC3a>();
			}

			string formObj = DbQualifier.Qualify(databaseName, "FormC3a");
			string contractObj = DbQualifier.Qualify(databaseName, "Contract");
			var sql = @$"SELECT c.* FROM {formObj} c
					LEFT JOIN {contractObj} cc ON c.ContractId = cc.Id
					CROSS APPLY STRING_SPLIT(cc.Owner, ',') owners
					WHERE (cc.Author IN @orgList or owners.value IN @orgList) {predicate}";

			using (IDbConnection db = new SqlConnection(_connectionString))
			{
				return db.Query<FormC3a>(sql, new { orgList }).ToList();
			}
		}

		public IEnumerable<FormC3a> GetAll()
		{
			using (IDbConnection db = new SqlConnection(_connectionString))
			{
				return db.Query<FormC3a>(sqlStrStart).ToList();
			}
		}

		public IEnumerable<FormC3a> GetAll(string? databaseName)
		{
			string sqlStart = sqlStrStart.Replace("FROM FormC3a c", $"FROM {DbQualifier.Qualify(databaseName, "FormC3a")} c");
			using (IDbConnection db = new SqlConnection(_connectionString))
			{
				return db.Query<FormC3a>(sqlStart).ToList();
			}
		}

		public FormC3a GetById(int id)
		{
			if (id > 0)
			{
				using (IDbConnection db = new SqlConnection(_connectionString))
				{
					return db.Query<FormC3a>(@$"{sqlStrStart} WHERE c.Id = @id", new { id }).FirstOrDefault();
				}
			}
			return null;
		}

		public FormC3a GetById(int id, string? databaseName)
		{
			if (id > 0)
			{
				string sqlStart = sqlStrStart.Replace("FROM FormC3a c", $"FROM {DbQualifier.Qualify(databaseName, "FormC3a")} c");
				using (IDbConnection db = new SqlConnection(_connectionString))
				{
					return db.Query<FormC3a>(@$"{sqlStart} WHERE c.Id = @id", new { id }).FirstOrDefault();
				}
			}
			return null;
		}

		public IEnumerable<FormC3a> GetEntitySkipTake(int skip, int take, string organizationName)
		{
			var sql = @$"SELECT c.* FROM FormC3a c
					LEFT JOIN Contract cc ON c.ContractId = cc.Id
					CROSS APPLY STRING_SPLIT(cc.Owner, ',') owners
					WHERE (cc.Author IN @orgList or owners.value IN @orgList)
					ORDER BY c.Id DESC
					OFFSET @skip ROWS
					FETCH NEXT @take ROWS ONLY";

			string[] orgList = organizationName.Split(',');

			using (IDbConnection db = new SqlConnection(_connectionString))
			{
				return db.Query<FormC3a>(sql, new { skip, take, orgList }).ToList();
			}
		}

		public IEnumerable<FormC3a> GetEntitySkipTake(int skip, int take, string organizationName, string? databaseName)
		{
			var sql = @$"SELECT c.* FROM {DbQualifier.Qualify(databaseName, "FormC3a")} c
					LEFT JOIN {DbQualifier.Qualify(databaseName, "Contract")} cc ON c.ContractId = cc.Id
					CROSS APPLY STRING_SPLIT(cc.Owner, ',') owners
					WHERE (cc.Author IN @orgList or owners.value IN @orgList)
					ORDER BY c.Id DESC
					OFFSET @skip ROWS
					FETCH NEXT @take ROWS ONLY";

			string[] orgList = organizationName.Split(',');

			using (IDbConnection db = new SqlConnection(_connectionString))
			{
				return db.Query<FormC3a>(sql, new { skip, take, orgList }).ToList();
			}
		}
	}
}


