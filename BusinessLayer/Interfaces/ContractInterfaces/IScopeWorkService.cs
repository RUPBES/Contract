using BusinessLayer.Enums;
using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Models.KDO;

namespace BusinessLayer.Interfaces.ContractInterfaces
{
    public interface IScopeWorkService : IService<ScopeWorkDTO, ScopeWork>
    {
        void AddAmendmentToScopeWork(int amendmentId, int scopeworkId);
        (DateTime, DateTime)? GetPeriodRangeScopeWork(int contractId);
        AmendmentDTO? GetAmendmentByScopeId(int scopeId);
        IEnumerable<AmendmentDTO> GetFreeAmendment(int contractId);
        ScopeWorkDTO GetLastScope(int contractId, bool isOwnForces = false);
        ScopeWorkDTO GetScopeByAmendment(int amendmentId);
        AmendmentDTO GetLastAmendmentWithScope(int contractId);

        void AddOwnForcesCostsByScopeId(ScopeWorkDTO scopeWork, int operatorSign = 1);      
        bool EditCostMainContract(int multipleContractId, int subObjId, ContractType type);                
        void UpdateParentCosts(int parentContrId, List<SWCostDTO> costs, bool isOwnForces, int operatorSign, int? changeScopeId = null);
        void RemoveSubContractCost(int costId, int contractId, Dictionary<int, ContractType> parentContracts, int operatorSign = -1);
    }
}
