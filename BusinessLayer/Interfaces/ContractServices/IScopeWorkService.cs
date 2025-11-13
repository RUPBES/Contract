using BusinessLayer.Enums;
using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Models.KDO;
using DatabaseLayer.Models.KDO;

namespace BusinessLayer.Interfaces.ContractInterfaces
{
    public interface IScopeWorkService : IService<ScopeWorkDTO, ScopeWork>
    {
        void AddAmendment(int amendmentId, int scopeworkId);
        (DateTime StartDate, DateTime EndDate)? GetScopeWorkPeriodRange(int contractId);
        AmendmentDTO? GetAmendmentByScopeId(int scopeId, bool? useArchiveData = null);
        IEnumerable<AmendmentDTO> GetFreeAmendment(int contractId);
        ScopeWorkDTO GetLastScope(int contractId, bool isOwnForces = false, bool? useArchiveData = null);
        ScopeWorkDTO GetByAmendmentId(int amendmentId);       
        ScopeWorkReportModel GetScopeWorksInfoTable(int contractId, ScopeType type = ScopeType.NoOwn, bool? useArchiveData = null);
        bool? HasNewAmendment(int contractId);
        bool TryUpdateParentsScopeCosts(ScopeWorkDTO scope, Dictionary<int, Enums.Contract>? parentContracts, CrudOp method, List<SWCostDTO>? previousScopeId = null, bool isOneOfMultipleDelete = false);
    }
}
