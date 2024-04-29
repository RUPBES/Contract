using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Models.KDO;

namespace BusinessLayer.Interfaces.ContractInterfaces
{
    public interface IActService : IService<ActDTO, Act>
    {
        void AddFile(int actId, int fileId);
    }
}
