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
        ScopeWork GetLastScope(int contractId, bool isOwnForces = false);
        ScopeWork GetScopeByAmendment(int amendmentId);

        void AddOwnForcesCostsByScopeId(ScopeWorkDTO scopeWork, int operatorSign = 1);
        void AddOrSubstractCostsOwnForceMnContract(int? mainOwnContrId, List<SWCostDTO> cost, int addOrSubstr);
        void RemoveCostsOfMainContract(int multipleContractId, int subObjId);
        void RemoveOneCostOfMainContract(int? mainContractScopeId, SWCostDTO cost);
        void RemoveExistOwnForce(int mainScopeId, int swCostId);
        bool DeleteAllScopeWorkContract(int scopeWorkId);
        void UpdateParentCosts(int parentContrId, List<SWCostDTO> costs, bool isOwnForces, int operatorSign, int? changeScopeId = null);
        void RemoveSubContractCost(int costId, int contractId, Dictionary<int, ContractType> parentContracts, int operatorSign = -1);
    }
}
