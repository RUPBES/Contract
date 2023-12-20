
using BusinessLayer.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MvcLayer.Models
{
    public class EmployeeViewModel
    {
        public int Id { get; set; }

        [DisplayName("Имя")]
        [RegularExpression("^[А-Яа-яЁё\\s]+$", ErrorMessage = "Прописные и строчные буквы русского алфавита")]
        [Required(ErrorMessage = "Необходимо заполнить Имя")]
        public string? FirstName { get; set; }

        [DisplayName("Фамилия")]
        [RegularExpression("^[А-Яа-яЁё\\s]+$", ErrorMessage = "Прописные и строчные буквы русского алфавита")]
        [Required(ErrorMessage = "Необходимо заполнить Фамилию")]
        public string? LastName { get; set; }

        [DisplayName("Отчество")]
        [RegularExpression("^[А-Яа-яЁё\\s]+$", ErrorMessage = "Прописные и строчные буквы русского алфавита")]
        [Required(ErrorMessage = "Необходимо заполнить Отчество")]
        public string? FatherName { get; set; }

        [DisplayName("Фамилия, имя, отчество")]
        public string? FullName { get; set; }

        [DisplayName("Фамилия и инициалы")]
        public string? Fio { get; set; }

        [DisplayName("Должность")]
        public string? Position { get; set; }

        [DisplayName("Электронная почта")]
        public string? Email { get; set; }
        public string? Author { get; set; }

        public List<EmployeeContractDTO> EmployeeContracts { get; set; } = new List<EmployeeContractDTO>();
               
        public List<PhoneViewModel> Phones { get; set; } = new List<PhoneViewModel>();

        public List<DepartmentEmployeeDTO> DepartmentEmployees { get; set; } = new List<DepartmentEmployeeDTO>();
     }
}
