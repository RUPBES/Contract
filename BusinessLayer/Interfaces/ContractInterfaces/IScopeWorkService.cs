using BusinessLayer.Enums;
using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Models.KDO;

namespace BusinessLayer.Interfaces.ContractInterfaces
{
    public interface IScopeWorkService : IService<ScopeWorkDTO, ScopeWork>
    {
        void AddAmendment(int amendmentId, int scopeworkId);
        (DateTime StartDate, DateTime EndDate)? GetScopeWorkPeriodRange(int contractId);
        AmendmentDTO? GetAmendmentByScopeId(int scopeId);
        IEnumerable<AmendmentDTO> GetFreeAmendment(int contractId);
        ScopeWorkDTO GetLastScope(int contractId, bool isOwnForces = false);
        ScopeWorkDTO GetByAmendmentId(int amendmentId);       
        ScopeWorkReportModel GetScopeWorksInfoTable(int contractId, ScopeType type = ScopeType.NoOwn);
        bool? HasNewAmendment(int contractId);
        bool TryUpdateParentsScopeCosts(ScopeWorkDTO scope, Dictionary<int, ContractType>? parentContracts, CrudOp method, List<SWCostDTO>? previousScopeId = null, bool isOneOfMultipleDelete = false);
    }
}
