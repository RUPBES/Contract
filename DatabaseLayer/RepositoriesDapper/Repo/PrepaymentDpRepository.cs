using Dapper;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.KDO;
using Microsoft.Data.SqlClient;
using System.Data;
using DatabaseLayer.RepositoriesDapper.Sql;

namespace DatabaseLayer.RepositoriesDapper.Repo
{
	public class PrepaymentDpRepository : IReadonlyRepoDapper<Prepayment>
	{
		private readonly string? _connectionString = null;
		private readonly string sqlStrStart = @"SELECT * FROM Prepayment c";

		public PrepaymentDpRepository(string connectionString)
		{
			_connectionString = connectionString;
		}

		public int Count()
		{
			using (IDbConnection db = new SqlConnection(_connectionString))
			{
				return db.Query<int>("SELECT COUNT(Id) FROM Prepayment").FirstOrDefault();
			}
		}

		public int Count(string? databaseName)
		{
			string obj = DbQualifier.Qualify(databaseName, "Prepayment");
			using (IDbConnection db = new SqlConnection(_connectionString))
			{
				return db.Query<int>($"SELECT COUNT(Id) FROM {obj}").FirstOrDefault();
			}
		}

		public IEnumerable<Prepayment> Find(string predicate)
		{
			if (string.IsNullOrEmpty(predicate))
			{
				return Array.Empty<Prepayment>();
			}

			using (IDbConnection db = new SqlConnection(_connectionString))
			{
				return db.Query<Prepayment>($"{sqlStrStart} {predicate}").ToList();
			}
		}

		public IEnumerable<Prepayment> Find(string predicate, string? databaseName)
		{
			if (string.IsNullOrEmpty(predicate))
			{
				return Array.Empty<Prepayment>();
			}

			string sqlStart = sqlStrStart.Replace("FROM Prepayment c", $"FROM {DbQualifier.Qualify(databaseName, "Prepayment")} c");
			using (IDbConnection db = new SqlConnection(_connectionString))
			{
				return db.Query<Prepayment>($"{sqlStart} {predicate}").ToList();
			}
		}

		public IEnumerable<Prepayment> Find(string predicate, string[] orgList)
		{
			if (string.IsNullOrEmpty(predicate))
			{
				return Array.Empty<Prepayment>();
			}

			using (IDbConnection db = new SqlConnection(_connectionString))
			{
				var sql = @$"SELECT c.* FROM Prepayment c
						LEFT JOIN Contract cc ON c.ContractId = cc.Id
						CROSS APPLY STRING_SPLIT(cc.Owner, ',') owners
						WHERE (cc.Author IN @orgList or owners.value IN @orgList) {predicate}";
				return db.Query<Prepayment>(sql, new { orgList }).ToList();
			}
		}

		public IEnumerable<Prepayment> Find(string predicate, string[] orgList, string? databaseName)
		{
			if (string.IsNullOrEmpty(predicate))
			{
				return Array.Empty<Prepayment>();
			}

			string prepObj = DbQualifier.Qualify(databaseName, "Prepayment");
			string contractObj = DbQualifier.Qualify(databaseName, "Contract");
			var sql = @$"SELECT c.* FROM {prepObj} c
					LEFT JOIN {contractObj} cc ON c.ContractId = cc.Id
					CROSS APPLY STRING_SPLIT(cc.Owner, ',') owners
					WHERE (cc.Author IN @orgList or owners.value IN @orgList) {predicate}";

			using (IDbConnection db = new SqlConnection(_connectionString))
			{
				return db.Query<Prepayment>(sql, new { orgList }).ToList();
			}
		}

		public IEnumerable<Prepayment> GetAll()
		{
			using (IDbConnection db = new SqlConnection(_connectionString))
			{
				return db.Query<Prepayment>(sqlStrStart).ToList();
			}
		}

		public IEnumerable<Prepayment> GetAll(string? databaseName)
		{
			string sqlStart = sqlStrStart.Replace("FROM Prepayment c", $"FROM {DbQualifier.Qualify(databaseName, "Prepayment")} c");
			using (IDbConnection db = new SqlConnection(_connectionString))
			{
				return db.Query<Prepayment>(sqlStart).ToList();
			}
		}

		public Prepayment GetById(int id)
		{
			if (id > 0)
			{
				using (IDbConnection db = new SqlConnection(_connectionString))
				{
					return db.Query<Prepayment>(@$"{sqlStrStart} WHERE c.Id = @id", new { id }).FirstOrDefault();
				}
			}
			return null;
		}

		public Prepayment GetById(int id, string? databaseName)
		{
			if (id > 0)
			{
				string sqlStart = sqlStrStart.Replace("FROM Prepayment c", $"FROM {DbQualifier.Qualify(databaseName, "Prepayment")} c");
				using (IDbConnection db = new SqlConnection(_connectionString))
				{
					return db.Query<Prepayment>(@$"{sqlStart} WHERE c.Id = @id", new { id }).FirstOrDefault();
				}
			}
			return null;
		}

		public IEnumerable<Prepayment> GetEntitySkipTake(int skip, int take, string organizationName)
		{
			var sql = @$"SELECT c.* FROM Prepayment c
					LEFT JOIN Contract cc ON c.ContractId = cc.Id
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

		public IEnumerable<Prepayment> GetEntitySkipTake(int skip, int take, string organizationName, string? databaseName)
		{
			var sql = @$"SELECT c.* FROM {DbQualifier.Qualify(databaseName, "Prepayment")} c
					LEFT JOIN {DbQualifier.Qualify(databaseName, "Contract")} cc ON c.ContractId = cc.Id
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
}


