using Dapper;
using DatabaseLayer.Models.KDO;
using Microsoft.Data.SqlClient;
using System.Data;
using DatabaseLayer.RepositoriesDapper.Sql;
using DatabaseLayer.Interfaces.Dapper;

namespace DatabaseLayer.RepositoriesDapper.Repo
{
	public class ActDpRepository : IReadonlyRepoDapper<Act>
	{
		private readonly string? _connectionString = null;		
		public ActDpRepository(string connectionString)
		{
			_connectionString = connectionString;
		}

		public int Count(string[] orgList, string? databaseName)
		{
            string obj = DbQualifier.Qualify(databaseName, "Act");

            if (orgList is null || orgList?.Length == 0)
            {
                using (IDbConnection db = new SqlConnection(_connectionString))
                {
                    return db.Query<int>($"SELECT COUNT(Id) FROM {obj}").FirstOrDefault();
                }
            }


            string contractObj = DbQualifier.Qualify(databaseName, "Contract");

            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return db.Query<int>(@$"SELECT distinct COUNT(c.Id) FROM {obj} c
									LEFT JOIN {contractObj} cc ON c.ContractId = cc.Id 
									CROSS APPLY STRING_SPLIT(cc.Owner, ',') owners
									WHERE (cc.Author IN @orgList or owners.value IN @orgList)", new { orgList }).FirstOrDefault();
            }
		}
				
		public IEnumerable<Act> Find(string predicate, string[] orgList, string? databaseName)
		{
			if (string.IsNullOrEmpty(predicate))
			{
				return Array.Empty<Act>();
			}
						
			var sql = @$"SELECT distinct c.* FROM {	DbQualifier.Qualify(databaseName, "Act")	} c
					LEFT JOIN {DbQualifier.Qualify(databaseName, "Contract")	} cc ON c.ContractId = cc.Id
					CROSS APPLY STRING_SPLIT(cc.Owner, ',') owners
					WHERE (cc.Author IN @orgList or owners.value IN @orgList) {predicate}";

			using (IDbConnection db = new SqlConnection(_connectionString))
			{
				return db.Query<Act>(sql, new { orgList }).ToList();
			}
		}

		public IEnumerable<Act> GetAll(string? databaseName)
		{
			string obj = DbQualifier.Qualify(databaseName, "Act");
			using (IDbConnection db = new SqlConnection(_connectionString))
			{
				return db.Query<Act>(@$"SELECT distinct c.* FROM {obj} c").ToList();
			}
		}

		public Act GetById(int id, string? databaseName)
		{
			if (id > 0)
			{
				string obj = DbQualifier.Qualify(databaseName, "Act");
				using (IDbConnection db = new SqlConnection(_connectionString))
				{
					return db.Query<Act>(@$"SELECT c.* FROM {obj} c WHERE c.Id = @id", new { id }).FirstOrDefault();
				}
			}
			return null;
		}

		public IEnumerable<Act> GetEntitySkipTake(int skip, int take, string organizationName, string? databaseName)
		{
			var sql = @$"SELECT distinct c.* FROM {DbQualifier.Qualify(databaseName, "Act")} c
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


