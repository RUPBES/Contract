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
        IEnumerable<ContractDTO> Find(Func<Contract, bool> where, Func<Contract, Contract> select);
        IEnumerable<ContractDTO> GetPage(int pageSize, int pageNum, string filter, out int count, string org);
        IEnumerable<ContractDTO> GetPageFilter(int pageSize, int pageNum, string request, string filter, out int count, string org);
        

        //void DeleteScopeWorks(int id);        
       

        IEnumerable<ContractDTO> GetSubsByType(int? id, ContractType? contractType);

        bool IsContractNumberExists(string numberContract);

        int? GetPaymentDueDate(int contrId);
        int? GetPaymentDueDate(string? paymentDescription, string? subPaymentDescription);
        Dictionary<int, ContractType>? GetParents(int? contractId, out ContractType thisType);

        List<int> GetChildren(int contractId);

        //bool IsNotGenContract(int? contractId, out int mainContrId);
        //bool IsThereScopeWorks(int contarctId, bool isOwnForses, out int? scopeId);
        //ContractType GetContractType(int contractId, out int parentContrId);
    }
}