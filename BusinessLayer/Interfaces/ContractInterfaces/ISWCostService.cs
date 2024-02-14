using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Models;

namespace BusinessLayer.Interfaces.ContractInterfaces
{
    public interface ISWCostService : IService<SWCostDTO, SWCost>
    {
        IEnumerable<SWCostDTO> Find(Func<SWCost, bool> where, Func<SWCost, SWCost> select);
        (DateTime, DateTime)? GetPeriodRangeScopeWork(int contractId);
        List<SWCost>? GetValueScopeWorkByPeriod(int contractId, DateTime? start, DateTime? end, Boolean IsOwn = false);
        List<SWCost>? GetValueScopeWork(int contractId, Boolean IsOwn = false);
    }
}