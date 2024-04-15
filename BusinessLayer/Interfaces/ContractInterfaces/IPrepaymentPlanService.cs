using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Models.KDO;

namespace BusinessLayer.Interfaces.ContractInterfaces
{
    public interface IPrepaymentPlanService : IService<PrepaymentPlanDTO, PrepaymentPlan>
    {
        PrepaymentDTO GetLastPrepayment(int contractId);
    }
}
