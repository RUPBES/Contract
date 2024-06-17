using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Models.KDO;

namespace BusinessLayer.Interfaces.ContractInterfaces
{
    public interface IOrganizationService : IService<OrganizationDTO, Organization>
    {
        public IndexViewModel GetPageFilter(int pageSize, int pageNum, string request, string sortOrder);
        public IndexViewModel GetPage(int pageSize, int pageNum);
        OrganizationDTO GetByEmployeeId(int employeeId);
        string? GetNameByContractId(int contrId);
        OrganizationDTO FindByContractOrganization(Func<ContractOrganization, bool> predicate);
    }
}
