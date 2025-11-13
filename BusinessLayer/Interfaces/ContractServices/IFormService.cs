using BusinessLayer.Enums;
using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Models.KDO;
using DatabaseLayer.Models.KDO;

namespace BusinessLayer.Interfaces.ContractInterfaces
{
    public interface IFormService : IService<FormDTO, FormC3a>
    {
        IEnumerable<FormDTO> Find(Func<FormC3a, bool> where, Func<FormC3a, FormC3a> select);

        IEnumerable<DateTime> GetFreeForms(int contractId);

        List<FormDTO> GetNestedFormsByPeriodAndContrId(int contractId, DateTime period, bool? useArchiveData =null);

        ScopeWorkReportModel GetScopeWorksInfoTable(int contractId, ScopeType type, bool? useArchiveData = null);

        bool TryUpdateParentsForms(FormDTO newForm, Dictionary<int, Enums.Contract>? parentContracts, CrudOp operation, FormDTO? previousStateForm = null, bool isOneOfMultipleDelete = false);
    }
}