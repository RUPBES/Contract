using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Models.KDO;
using DatabaseLayer.Models.KDO;

namespace BusinessLayer.Interfaces.ContractInterfaces
{
    public interface IEmployeeService : IService<EmployeeDTO, Employee>
    {
        public IndexViewModel GetPage(int pageSize, int pageNum, string org);
        public IndexViewModel GetPageFilter(int pageSize, int pageNum, string request, string sortOrder, string org);
        EmployeeDTO FindByContractEmployee(Func<EmployeeContract, bool> predicate);
        List<EmployeeDTO> FindBestMatch(string inputName);
        EmployeeDTO? ParseString1CToEmployee(string employeeFrom1C);
        IndexViewModel Filter(int pageSize, int pageNum, string type, string? sortDirection, string org, string? searchText);
    }
}
