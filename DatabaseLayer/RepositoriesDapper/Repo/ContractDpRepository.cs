using Dapper;
using DatabaseLayer.Interfaces.Entities;
using DatabaseLayer.Models.KDO;
using Microsoft.Data.SqlClient;
using DatabaseLayer.RepositoriesDapper.Sql;
using System.Data;

namespace DatabaseLayer.RepositoriesDapper.Repo
{
    public class ContractDpRepository : IReadonlyContractDapperRepo
    {
        string _connectionString = null;
        private readonly string sqlStrStart = @$" SELECT [Id] ,[Number],[SubContractId] ,[AgreementContractId] ,[Date] ,[EnteringTerm]
                                ,[ContractTerm] ,[Сurrency] ,[ContractPrice] ,[NameObject] ,[FundingSource] ,[IsSubContract] ,[IsEngineering]
                                ,[IsAgreementContract] ,[PaymentСonditionsAvans] ,[PaymentСonditionsRaschet] ,[IsMultiple] ,[MultipleContractId]
                                ,[IsOneOfMultiple] ,[PaymentСonditionsPrice] ,[Author] ,[Owner] ,[IsExpired] ,[IsClosed] ,[IsArchive] [ArchivedDate]
                                    ,COALESCE(a.LastAmendmentPrice, c.ContractPrice) AS ContractPrice
                                    ,COALESCE(a.DateBeginWork, c.DateBeginWork) AS DateBeginWork   
                                    ,COALESCE(a.DateEndWork, c.DateEndWork) AS DateEndWork  
                                    ,COALESCE(a.DateEntryObject, c.EnteringTerm) AS EnteringTerm  
                                FROM Contract c
                                OUTER APPLY (
                                    SELECT TOP 1         
                                        ContractPrice AS LastAmendmentPrice,
                                        DateBeginWork,
                                        DateEndWork,
                                        DateEntryObject
                                    FROM Amendment a
                                    WHERE ContractId = c.Id
                                    ORDER BY a.[Date] DESC
                                ) a";
        private readonly string sqlStrDetailsStart = @"SELECT c.[Id] ,[Number] ,[SubContractId] ,[AgreementContractId] ,[Date],[ContractTerm],[Сurrency],[NameObject],[FundingSource]
                ,[IsSubContract] ,[IsEngineering],[IsAgreementContract],[IsMultiple] ,[MultipleContractId],[IsOneOfMultiple]
                ,[PaymentСonditionsAvans] ,[PaymentСonditionsRaschet] ,c.[Author] ,c.[Owner],[IsExpired],[IsClosed] ,[IsArchive],sp.TypeProcedure as ProcedureName, sp.Id as ProcedureId,
                (SELECT TOP 1 e.FIO FROM Employee e join EmployeeContract ec on ec.EmployeeId = e.Id WHERE ec.ContractId = c.Id and ec.[IsSignatory ] = 1 ) as SignatoryEmp
                ,( SELECT TOP 1 e.FIO FROM Employee e join EmployeeContract ec on ec.EmployeeId = e.Id WHERE ec.ContractId = c.Id and ec.IsResponsible = 1 ) as ResponsibleEmp
                ,t.Name as WorkType
                ,( SELECT TOP 1 o.Name FROM Organization o join ContractOrganization oc on oc.OrganizationId = o.Id WHERE oc.ContractId = c.Id and oc.IsClient = 1 ) as Client
                ,( SELECT TOP 1 o.Name FROM Organization o join ContractOrganization oc on oc.OrganizationId = o.Id WHERE oc.ContractId = c.Id and oc.IsGenContractor = 1 ) as GenContractor
                ,( SELECT TOP 1 o.Name FROM Organization o join ContractOrganization oc on oc.OrganizationId = o.Id WHERE oc.ContractId = c.Id and oc.IsResponsibleForWork = 1 ) as ResponsibleForWork
                ,COALESCE(a.LastAmendmentPrice, c.ContractPrice) AS ContractPrice
                ,COALESCE(a.DateBeginWork, c.DateBeginWork) AS DateBeginWork
                ,COALESCE(a.DateEndWork, c.DateEndWork) AS DateEndWork
                ,COALESCE(a.DateEntryObject, c.EnteringTerm) AS EnteringTerm
                    FROM Contract c
                    left join TypeWorkContract tc on tc.ContractId = c.Id
                    left join TypeWork t on t.Id = tc.TypeWorkId
                    left join SelectionProcedure sp on sp.ContractId = c.Id
                    OUTER APPLY (SELECT TOP 1 ContractPrice AS LastAmendmentPrice, DateBeginWork, DateEndWork, DateEntryObject FROM Amendment a WHERE ContractId = c.Id
                    ORDER BY a.[Date] DESC) a";

        public ContractDpRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public int Count()
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return db.Query<int>("SELECT COUNT(Id) FROM vContracts").FirstOrDefault();
            }
        }

        public int Count(string? databaseName)
        {
            string viewName = DbQualifier.Qualify(databaseName, "vContracts");
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return db.Query<int>($"SELECT COUNT(Id) FROM {viewName}").FirstOrDefault();
            }
        }

        public IEnumerable<Contract> Find(string predicate)
        {
            if (string.IsNullOrEmpty(predicate))
            {
                return Array.Empty<Contract>();
            }

            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return db.Query<Contract>($"{sqlStrStart} {predicate}").ToList();
            }
        }

        public IEnumerable<Contract> Find(string predicate, string? databaseName)
        {
            if (string.IsNullOrEmpty(predicate))
            {
                return Array.Empty<Contract>();
            }

            string sqlStart = sqlStrStart.Replace("FROM Contract c", $"FROM {DbQualifier.Qualify(databaseName, "Contract")} c");
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return db.Query<Contract>($"{sqlStart} {predicate}").ToList();
            }
        }

        public IEnumerable<Contract> Find(string predicate, string[] orgList)
        {
            if (string.IsNullOrEmpty(predicate))
            {
                return Array.Empty<Contract>();
            }

            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return db.Query<Contract>($"{sqlStrStart} CROSS APPLY STRING_SPLIT(c.Owner, ',') owners WHERE (Author IN @orgList or owners.value IN @orgList) {predicate}", new { orgList }).ToList();
            }
        }

        public IEnumerable<Contract> Find(string predicate, string[] orgList, string? databaseName)
        {
            if (string.IsNullOrEmpty(predicate))
            {
                return Array.Empty<Contract>();
            }

            string sqlStart = sqlStrStart.Replace("FROM Contract c", $"FROM {DbQualifier.Qualify(databaseName, "Contract")} c");
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return db.Query<Contract>($"{sqlStart} CROSS APPLY STRING_SPLIT(c.Owner, ',') owners WHERE (Author IN @orgList or owners.value IN @orgList) {predicate}", new { orgList }).ToList();
            }
        }

        public IEnumerable<Contract> GetAll()
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return db.Query<Contract>(sqlStrStart).ToList();
            }
        }

        public IEnumerable<Contract> GetAll(string? databaseName)
        {
            string sqlStart = sqlStrStart.Replace("FROM Contract c", $"FROM {DbQualifier.Qualify(databaseName, "Contract")} c");
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return db.Query<Contract>(sqlStart).ToList();
            }
        }

        public Contract GetById(int id)
        {
            if (id > 0)
            {
                using (IDbConnection db = new SqlConnection(_connectionString))
                {
                    return db.Query<Contract>(@$"{sqlStrStart} WHERE c.Id = @id", new { id }).FirstOrDefault();
                }
            }
            return null;
        }

        public Contract GetById(int id, string? databaseName)
        {
            if (id > 0)
            {
                string sqlStart = sqlStrStart.Replace("FROM Contract c", $"FROM {DbQualifier.Qualify(databaseName, "Contract")} c");
                using (IDbConnection db = new SqlConnection(_connectionString))
                {
                    return db.Query<Contract>(@$"{sqlStart} WHERE c.Id = @id", new { id }).FirstOrDefault();
                }
            }
            return null;
        }

        public VContract GetById(string whereStr)
        {
            if (!string.IsNullOrEmpty(whereStr))
            {
                using (IDbConnection db = new SqlConnection(_connectionString))
                {
                    return db.Query<VContract>(@$"{sqlStrDetailsStart} {whereStr}").FirstOrDefault();
                }
            }
            return null;
        }

        public VContract GetById(string whereStr, string? databaseName)
        {
            if (!string.IsNullOrEmpty(whereStr))
            {
                string sqlDetails = sqlStrDetailsStart.Replace("FROM Contract c", $"FROM {DbQualifier.Qualify(databaseName, "Contract")} c");
                using (IDbConnection db = new SqlConnection(_connectionString))
                {
                    return db.Query<VContract>(@$"{sqlDetails} {whereStr}").FirstOrDefault();
                }
            }
            return null;
        }

        public IEnumerable<Contract> GetEntitySkipTake(int skip, int take, string org)
        {
            var sql = @$"{sqlStrStart} CROSS APPLY STRING_SPLIT(c.Owner, ',') owners WHERE (Author IN @orgList or owners.value IN @orgList) 
                        ORDER BY Id DESC
                        OFFSET @skip ROWS
                        FETCH NEXT @take ROWS ONLY";

            string[] orgList = org.Split(',');

            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return db.Query<Contract>(sql, new { skip, take, orgList }).ToList();
            }
        }

        public IEnumerable<Contract> GetEntitySkipTake(int skip, int take, string org, string? databaseName)
        {
            string[] orgList = org.Split(',');

            var sql = @$"{sqlStrStart.Replace("FROM Contract c", $"FROM {DbQualifier.Qualify(databaseName, "Contract")} c")} CROSS APPLY STRING_SPLIT(c.Owner, ',') owners WHERE (Author IN @orgList or owners.value IN @orgList) 
                        ORDER BY Id DESC
                        OFFSET @skip ROWS
                        FETCH NEXT @take ROWS ONLY";

            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return db.Query<Contract>(sql, new { skip, take, orgList }).ToList();
            }
        }

        public IEnumerable<VContract> GetSubsById(int id, string where)
        {
            if (id > 0)
            {                
                using (IDbConnection db = new SqlConnection(_connectionString))
                {
                    return db.Query<VContract>(@$"{sqlStrDetailsStart} {where} ", new { id }).ToList();
                }
            }
            return Array.Empty<VContract>();
        }

        public IEnumerable<VContract> GetSubsById(int id, string where, string? databaseName)
        {
            if (id > 0)
            {
                string sqlDetails = sqlStrDetailsStart.Replace("FROM Contract c", $"FROM {DbQualifier.Qualify(databaseName, "Contract")} c");
                using (IDbConnection db = new SqlConnection(_connectionString))
                {
                    return db.Query<VContract>(@$"{sqlDetails} {where} ", new { id }).ToList();
                }
            }
            return Array.Empty<VContract>();
        }
    }
}
