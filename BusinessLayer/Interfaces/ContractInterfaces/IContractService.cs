using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Models;

namespace BusinessLayer.Interfaces.ContractInterfaces
{
    public interface IContractService : IService<ContractDTO, Contract>
    {
        List<ContractDTO>? ExistContractAndReturnListSameContracts(string numberContract, DateTime? dateContract);
        bool ExistContractByNumber(string numberContract);

        void AddFile(int contractId, int fileId);
        void DeleteAfterScopeWork(int id);
        public IEnumerable<ContractDTO> GetPageFilter(int pageSize, int pageNum, string request, out int count);
        public IEnumerable<ContractDTO> GetPage(int pageSize, int pageNum, out int count);
    }
}