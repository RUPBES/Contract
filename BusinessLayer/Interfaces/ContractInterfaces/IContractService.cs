using BusinessLayer.Enums;
using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Models.KDO;

namespace BusinessLayer.Interfaces.ContractInterfaces
{
    public interface IContractService : IService<ContractDTO, Contract>
    {
        IEnumerable<ContractDTO> Find(Func<Contract, bool> where, Func<Contract, Contract> select);
        List<ContractDTO>? ExistContractAndReturnListSameContracts(string numberContract, DateTime? dateContract);
        bool ExistContractByNumber(string numberContract);

        void AddFile(int contractId, int fileId);
        void DeleteScopeWorks(int id);
        void DeleteAfterScopeWork(int id);
        public IEnumerable<ContractDTO> GetPageFilter(int pageSize, int pageNum, string request, string filter, out int count, string org);
        public IEnumerable<ContractDTO> GetPage(int pageSize, int pageNum, string filter, out int count, string org);
        //////////////////////////////////////////
        ///
        bool IsNotGenContract(int? contractId, out int mainContrId);        
        bool IsThereScopeWorks(int contarctId, bool isOwnForses, out int? scopeId);  
        bool IsThereAmendment(int contarctId);
        int? GetDayOfRaschet(int contrId);
        (bool isExistChild, int id) IsHaveChild(int id);
        Dictionary<int, ContractType>? GetParentsList(ContractDTO? contract);

        ContractType GetContractType(ContractDTO? contract, out int parentContrId);
    }
}