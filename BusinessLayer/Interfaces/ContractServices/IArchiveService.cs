using BusinessLayer.Enums;
using BusinessLayer.Models.KDO;
using DatabaseLayer.Models.KDO;

namespace BusinessLayer.Interfaces.ContractInterfaces
{
    public interface IArchiveService
    {
        IEnumerable<VContractDTO> FindVContract(Func<VContract, bool> predicate);
       
        IEnumerable<VContractDTO> FindContractByQuery(string queryString);
        IEnumerable<VContractDTO> FindLikeNameObj(string queryString);
        IEnumerable<VContractDTO> GetAllContracts();
        ContractDTO GetContractById(int id);
        IndexViewModel GetContractPage(int pageSize, int pageNum, string org);
        IndexViewModel GetContractPageFilter(int pageSize, int pageNum, string request, string typeRequest, string sortOrder, string org);

        IEnumerable<ContractDTO> GetSubsByType(int? id, Enums.ContractType? contractType);
        TypeWorkDTO GetTypeWorkByContractId(int contractId);
        OrganizationDTO FindByContractOrganization(Func<ContractOrganization, bool> predicate);
        EmployeeDTO FindByContractEmployee(Func<EmployeeContract, bool> predicate);
        IEnumerable<SelectionProcedureDTO> FindSlctProcedure(Func<SelectionProcedure, bool> predicate);

    }
}
