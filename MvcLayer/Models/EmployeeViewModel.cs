
using BusinessLayer.Models;
using System.ComponentModel;

namespace MvcLayer.Models
{
    public class EmployeeViewModel
    {
        public int Id { get; set; }

        [DisplayName("Имя")]
        public string? FirstName { get; set; }

        [DisplayName("Фамилия")]
        public string? LastName { get; set; }

        [DisplayName("Отчество")]
        public string? FatherName { get; set; }

        [DisplayName("Фамилия, имя, отчество")]
        public string? FullName { get; set; }

        [DisplayName("Фамилия и инициалы")]
        public string? Fio { get; set; }

        [DisplayName("Должность")]
        public string? Position { get; set; }

        [DisplayName("Электронная почта")]
        public string? Email { get; set; }

        public List<EmployeeContractDTO> EmployeeContracts { get; set; } = new List<EmployeeContractDTO>();
               
        public List<PhoneViewModel> Phones { get; set; } = new List<PhoneViewModel>();

        public List<DepartmentViewModel> Departments { get; set; } = new List<DepartmentViewModel>();
    }
}
