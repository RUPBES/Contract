using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Models;

namespace BusinessLayer.Interfaces.ContractInterfaces
{
    public interface IAmendmentService : IService<AmendmentDTO, Amendment>
    {
        void AddFile(int amendId, int fileId);
    }
}
