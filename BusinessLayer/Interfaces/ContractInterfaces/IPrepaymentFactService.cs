using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Models.KDO;

namespace BusinessLayer.Interfaces.ContractInterfaces
{
    public interface IPrepaymentFactService : IService<PrepaymentFactDTO, PrepaymentFact>
    {
        Prepayment GetLastPrepayment(int contractId);
    }
}
