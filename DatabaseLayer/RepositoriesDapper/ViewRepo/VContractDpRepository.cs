using Dapper;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Models.KDO;
using Microsoft.Data.SqlClient;
using System.Data;

namespace DatabaseLayer.RepositoriesDapper.ViewRepo
{
    public class VContractDpRepository : IReadonlyRepoDapper<VContract>
    {
        private readonly string _connectionString = null;
        private readonly string sqlStrStart = @$" SELECT c.Id ,c.Number ,c.[ProcedureName] ,c.[SignatoryEmp] ,c.[ResponsibleEmp] ,c.[GenContractor]
                       ,c.[Client] ,c.[ResponsibleForWork] ,c.[SubContractId] ,c.[AgreementContractId] ,c.[Date] ,c.[ContractTerm] 
                        ,c.[Сurrency] ,c.[WorkType] ,c.[NameObject] ,c.[FundingSource] ,c.[PaymentСonditionsAvans] ,c.[PaymentСonditionsRaschet]
                        ,c.[Author] ,c.[Owner] ,c.[IsClosed] ,c.[IsExpired] ,c.[IsArchive] ,c.[PreYearSum] ,c.[RemainingSum] ,c.[ThisYearSum] , c.ProcedureId
                        , COALESCE(a.LastAmendmentPrice, c.ContractPrice) AS ContractPrice
                        ,COALESCE(a.DateBeginWork, c.DateBeginWork) AS DateBeginWork   
                        ,COALESCE(a.DateEndWork, c.DateEndWork) AS DateEndWork  
                        ,COALESCE(a.DateEntryObject, c.EnteringTerm) AS EnteringTerm  

                        FROM vContracts c
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

        public VContractDpRepository(string connectionString)
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

        public IEnumerable<VContract> Find(string predicate)
        {
            if (string.IsNullOrEmpty(predicate))
            {
                return Array.Empty<VContract>();
            }

            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return db.Query<VContract>($"{sqlStrStart} {predicate}").ToList();
            }
        }

        public IEnumerable<VContract> Find(string predicate, string[] orgList)
        {
            if (string.IsNullOrEmpty(predicate))
            {
                return Array.Empty<VContract>();
            }
            
            using (IDbConnection db = new SqlConnection(_connectionString))
            {                
                return db.Query<VContract>($"SELECT * FROM vContracts c where {predicate}", new {orgList}).ToList();
            }
        }

        public IEnumerable<VContract> GetAll()
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return db.Query<VContract>(sqlStrStart).ToList();
            }
        }

        public VContract GetById(int id)
        {
            if (id > 0)
            {
                var sql = @$"{sqlStrStart} WHERE c.Id = @id";
               
                using (IDbConnection db = new SqlConnection(_connectionString))
                {
                    return db.Query<VContract>(sql, new { id }).FirstOrDefault();
                }
            }
            return null;
        }

        public IEnumerable<VContract> GetEntitySkipTake(int skip, int take, string org)
        {
            string[] orgList = org.Split(',');

            var sql = @$"{sqlStrStart}
                        CROSS APPLY STRING_SPLIT(c.Owner, ',') owners
                        WHERE (Author IN @orgList or owners.value IN @orgList)
                        ORDER BY Id DESC
                        OFFSET @skip ROWS
                        FETCH NEXT @take ROWS ONLY";

            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                return db.Query<VContract>(sql, new { skip, take, orgList }).ToList();
            }
        }
    }
}
