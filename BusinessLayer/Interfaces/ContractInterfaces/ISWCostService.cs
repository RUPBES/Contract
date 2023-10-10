using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Models;

namespace BusinessLayer.Interfaces.ContractInterfaces
{
    public interface ISWCostService : IService<SWCostDTO, SWCost>
    {
        (DateTime, DateTime)? GetPeriodRangeScopeWork(int contractId);
    }
}