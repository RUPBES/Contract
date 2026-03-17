using Dapper;
using DatabaseLayer.Models.KDO;
using Microsoft.Data.SqlClient;
using System.Data;
using DatabaseLayer.RepositoriesDapper.Sql;
using DatabaseLayer.Interfaces.Dapper;

namespace DatabaseLayer.RepositoriesDapper.Repo
{
	public class FormC3DpRepository : IReadonlyRepoDapper<FormC3a>
	{
		private readonly string? _connectionString = null;
		public FormC3DpRepository(string connectionString)
		{
			_connectionString = connectionString;
		}

		public int Count(string[] orgList, string? databaseName)
		{
            string obj = DbQualifier.Qualify(databaseName, "FormC3a");

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
                return db.Query<int>(@$"SELECT distinct COUNT(Id) FROM {obj} c
									LEFT JOIN {contractObj} cc ON c.ContractId = cc.Id 
									CROSS APPLY STRING_SPLIT(cc.Owner, ',') owners
									WHERE (cc.Author IN @orgList or owners.value IN @orgList)", new { orgList }).FirstOrDefault();
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

		public IEnumerable<FormC3a> GetAll(string? databaseName)
		{
            string obj = DbQualifier.Qualify(databaseName, "FormC3a");

			using (IDbConnection db = new SqlConnection(_connectionString))
			{
				return db.Query<FormC3a>(@$"SELECT distinct c.* FROM {obj} c").ToList();
			}
		}

		public FormC3a GetById(int id, string? databaseName)
		{
			if (id > 0)
			{
                string obj = DbQualifier.Qualify(databaseName, "FormC3a");
                
				using (IDbConnection db = new SqlConnection(_connectionString))
				{
					return db.Query<FormC3a>(@$"SELECT c.* FROM {obj} c WHERE c.Id = @id", new { id }).FirstOrDefault();
				}
			}
			return null;
		}

		public IEnumerable<FormC3a> GetEntitySkipTake(int skip, int take, string organizationName, string? databaseName)
		{
			var sql = @$"SELECT distinct c.* FROM {DbQualifier.Qualify(databaseName, "FormC3a")} c
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