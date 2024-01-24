using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Models;

namespace BusinessLayer.Interfaces.ContractInterfaces
{
    public interface IFormService : IService<FormDTO, FormC3a>
    {
        void AddFile(int formId, int fileId);
        List<FormDTO> GetNestedFormsByPeriodAndContrId(int contractId, DateTime period);
        //FormDTO? GetValueScopeWorkByPeriod(int contractId, DateTime? period, Boolean IsOwn = false);
    }
}