﻿using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Models;

namespace BusinessLayer.Interfaces.ContractInterfaces
{
    public interface IScopeWorkService:IService<ScopeWorkDTO, ScopeWork>
    {
        void AddAmendmentToScopeWork(int amendmentId, int scopeworkId);
        (DateTime, DateTime)? GetPeriodRangeScopeWork(int contractId);
        (DateTime, DateTime)? GetFullPeriodRangeScopeWork(int contractId);
        AmendmentDTO? GetAmendmentByScopeId(int scopeId);
        IEnumerable<AmendmentDTO> GetFreeAmendment(int contractId);
        ScopeWork GetLastScope(int contractId);
        ScopeWork GetScopeByAmendment(int amendmentId);

        void AddSWCostForMainContract(int? scopeId, List<SWCostDTO> costs);
        void CreateSWCostForMainContract(int? scopeId, List<SWCostDTO> costs, bool isOwnForces);
        void SubstractSWCostForMainContract(int? scopeMainContractId, int changeScopeId, List<SWCostDTO> costs);
        void RemoveSWCostFromMainContract(int multipleContractId, int subObjId);
        void SubstractCostFromMainContract(int? mainContractScopeId, SWCostDTO cost);
    }
}
