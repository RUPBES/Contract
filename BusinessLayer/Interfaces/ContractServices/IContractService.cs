using BusinessLayer.Enums;
using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Models.KDO;
using BusinessLayer.Services;
using DatabaseLayer.Models.KDO;
using static BusinessLayer.Services.ContractService;

namespace BusinessLayer.Interfaces.ContractInterfaces
{
    public interface IContractService : IService<ContractDTO, DatabaseLayer.Models.KDO.Contract>
    {
        IEnumerable<ContractDTO> Find(Func<DatabaseLayer.Models.KDO.Contract, bool> where, Func<DatabaseLayer.Models.KDO.Contract, DatabaseLayer.Models.KDO.Contract> select, bool useArchiveData = false);

        IEnumerable<ContractDTO> GetPage(int pageSize, int pageNum, string filter, out int count, string org, bool useArchiveData = false);

        IEnumerable<ContractDTO> GetPageFilter(int pageSize, int pageNum, string request, string filter, out int count, string org, bool useArchiveData = false);
        
        IEnumerable<ContractDTO> GetSubsByType(int? id, Enums.ContractType? contractType, bool useArchiveData = false);

        ContractDTO GetById(int id, int? secondId = null, bool useArchiveData = false);

        bool IsContractNumberExists(string numberContract);

        int? GetPaymentDueDate(int contrId, bool? useArchiveData = null);

        int? GetPaymentDueDate(string? paymentDescription, string? subPaymentDescription);

        Dictionary<int, Enums.ContractType>? GetParents(int? contractId, out Enums.ContractType thisType);

        List<int> GetChildren(int contractId);

        Task<bool> MoveToArchive(int contrId);
        Task<int> Restructure(int contrId);
    }
}