using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Models.KDO;

namespace BusinessLayer.Interfaces.ContractInterfaces
{
    public interface IAmendmentService : IService<AmendmentDTO, Amendment>
    {
        void AddFile(int amendId, int fileId);
        IEnumerable<AmendmentDTO> Find(Func<Amendment, bool> where, Func<Amendment, Amendment> select);
    }
}
