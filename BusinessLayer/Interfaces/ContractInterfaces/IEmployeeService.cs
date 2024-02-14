using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Models;
using DatabaseLayer.Models;

namespace BusinessLayer.Interfaces.ContractInterfaces
{
    public interface IEmployeeService : IService<EmployeeDTO, Employee>
    {
        public IndexViewModel GetPage(int pageSize, int pageNum, string org);
        public IndexViewModel GetPageFilter(int pageSize, int pageNum, string request, string sortOrder, string org);
    }
}
