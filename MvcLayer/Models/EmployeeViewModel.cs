
using BusinessLayer.Models;

namespace MvcLayer.Models
{
    public class EmployeeViewModel
    {
        public int Id { get; set; }

        public string? FullName { get; set; }

        public string? Fio { get; set; }

        public string? Position { get; set; }

        public string? Email { get; set; }

        public List<EmployeeContractDTO> EmployeeContracts { get; set; } = new List<EmployeeContractDTO>();
               
        public List<PhoneViewModel> Phones { get; set; } = new List<PhoneViewModel>();

        public List<DepartmentViewModel> Departments { get; set; } = new List<DepartmentViewModel>();
    }
}
