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
        void DeleteScopeWorks(int id);
        void DeleteAfterScopeWork(int id);
        public IEnumerable<ContractDTO> GetPageFilter(int pageSize, int pageNum, string request, string filter, out int count, string org);
        public IEnumerable<ContractDTO> GetPage(int pageSize, int pageNum, string filter, out int count, string org);
        //////////////////////////////////////////
        ///
        bool IsThereSubObjs(int contarctId);
        bool IsThereScopeWorks(int contarctId, out int? scopeId);
        bool IsThereScopeWorks(int contarctId, bool isOwnForses, out int? scopeId);
        bool IsThereSWCosts(int? scopeId);
        bool IsThereAmendment(int contarctId);
        int? GetDayOfRaschet(int contrId);
    }
}