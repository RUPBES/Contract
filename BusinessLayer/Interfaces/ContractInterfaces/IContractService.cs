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
    }
}