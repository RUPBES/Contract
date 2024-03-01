using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Models;

namespace BusinessLayer.Interfaces.ContractInterfaces
{
    public interface IFormService : IService<FormDTO, FormC3a>
    {
        IEnumerable<FormDTO> Find(Func<FormC3a, bool> where, Func<FormC3a, FormC3a> select);
        void AddFile(int formId, int fileId);
        IEnumerable<DateTime> GetFreeForms(int contractId);
        List<FormDTO> GetNestedFormsByPeriodAndContrId(int contractId, DateTime period);
        void UpdateOwnForceMnForm(FormDTO newForm, bool isOnePartOfMultiContr = false, FormDTO? updateForm = null);
        //FormDTO? GetValueScopeWorkByPeriod(int contractId, DateTime? period, Boolean IsOwn = false);
    }
}