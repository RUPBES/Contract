using Dapper;
using DatabaseLayer.Models.KDO;
using Microsoft.Data.SqlClient;
using System.Data;
using DatabaseLayer.RepositoriesDapper.Sql;
using DatabaseLayer.Interfaces.Dapper;

namespace DatabaseLayer.RepositoriesDapper.Repo
{
	public class AmendmentDpRepository : IReadonlyRepoDapper<Amendment>
	{
		private readonly string? _connectionString = null;
		public AmendmentDpRepository(string connectionString)
		{
			_connectionString = connectionString;
		}

		public int Count(string[] orgList, string? databaseName)
		{
            string amendObj = DbQualifier.Qualify(databaseName, "Amendment");            

            if (orgList is null || orgList?.Length == 0)
            {
                using (IDbConnection db = new SqlConnection(_connectionString))
                {
                    return db.Query<int>($"SELECT COUNT(Id) FROM {amendObj}").FirstOrDefault();
                }
            }


            string contractObj = DbQualifier.Qualify(databaseName, "Contract");

            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return db.Query<int>(@$"SELECT distinct COUNT(c.Id) FROM {amendObj} c
									LEFT JOIN {contractObj} cc ON c.ContractId = cc.Id 
									CROSS APPLY STRING_SPLIT(cc.Owner, ',') owners
									WHERE (cc.Author IN @orgList or owners.value IN @orgList)", new { orgList }).FirstOrDefault();
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
			var sql = @$"SELECT distinct c.* FROM {amendObj} c
					LEFT JOIN {contractObj} cc ON c.ContractId = cc.Id
					CROSS APPLY STRING_SPLIT(cc.Owner, ',') owners
					WHERE (cc.Author IN @orgList or owners.value IN @orgList) {predicate}";

			using (IDbConnection db = new SqlConnection(_connectionString))
			{
				return db.Query<Amendment>(sql, new { orgList }).ToList();
			}
		}

		public IEnumerable<Amendment> GetAll(string? databaseName)
		{
            string amendObj = DbQualifier.Qualify(databaseName, "Amendment");
           
			using (IDbConnection db = new SqlConnection(_connectionString))
			{
				return db.Query<Amendment>(@$"SELECT distinct c.* FROM {amendObj} c").ToList();
			}
		}

		public Amendment GetById(int id, string? databaseName)
		{
			if (id > 0)
			{
                string amendObj = DbQualifier.Qualify(databaseName, "Amendment");
               
				using (IDbConnection db = new SqlConnection(_connectionString))
				{
					return db.Query<Amendment>(@$"SELECT c.* FROM {amendObj} c WHERE c.Id = @id", new { id }).FirstOrDefault();
				}
			}
			return null;
		}

		public IEnumerable<Amendment> GetEntitySkipTake(int skip, int take, string organizationName, string? databaseName)
		{
			var sql = @$"SELECT distinct c.* FROM {DbQualifier.Qualify(databaseName, "Amendment")} c
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


