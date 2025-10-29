using BusinessLayer.Enums;
using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Models;
using BusinessLayer.Services;
using DatabaseLayer.Models.KDO;
using static BusinessLayer.Services.ContractService;

namespace BusinessLayer.Interfaces.ContractInterfaces
{
    public interface IContractService : IService<ContractDTO, Contract>
    {
        IEnumerable<ContractDTO> Find(Func<Contract, bool> where, Func<Contract, Contract> select, bool useArchiveData = false);

        IEnumerable<ContractDTO> GetPage(int pageSize, int pageNum, string filter, out int count, string org, bool useArchiveData = false);

        IEnumerable<ContractDTO> GetPageFilter(int pageSize, int pageNum, string request, string filter, out int count, string org, bool useArchiveData = false);
        
        IEnumerable<ContractDTO> GetSubsByType(int? id, ContractType? contractType, bool useArchiveData = false);

        ContractDTO GetById(int id, int? secondId = null, bool useArchiveData = false);

        bool IsContractNumberExists(string numberContract);

        int? GetPaymentDueDate(int contrId, bool? useArchiveData = null);

        int? GetPaymentDueDate(string? paymentDescription, string? subPaymentDescription);

        Dictionary<int, ContractType>? GetParents(int? contractId, out ContractType thisType);

        List<int> GetChildren(int contractId);

        Task<bool> MoveToArchive(int contrId, string user, string sourceDB = "ContrTest", string targetDB = "ContrArchiveTest");
        Task<int> Restructure(int contrId, string user);
    }
}