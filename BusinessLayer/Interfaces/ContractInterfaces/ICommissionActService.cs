using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Models.KDO;

namespace BusinessLayer.Interfaces.ContractInterfaces
{
    public interface ICommissionActService : IService<CommissionActDTO, CommissionAct>
    {
        void AddFile(int commissionActId, int fileId);
    }
}
