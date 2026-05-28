using System.ComponentModel.DataAnnotations;

namespace BusinessLayer.Models.KDO
{
    public class EmployeeDTO
    {
        public int Id { get; set; }

        public string? FullName { get; set; }

        [Required(ErrorMessage = "Имя обязательно для заполнения")]
        [StringLength(80, MinimumLength = 2, ErrorMessage = "Имя должно быть от 2 до 80 символов")]
        [RegularExpression(@"^[A-Za-zА-ЯЁа-яё\s'-]+$",
     ErrorMessage = "Имя может содержать только буквы (русские и латинские) и апостроф")]
        [Display(Name = "Имя")]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "Фамилия обязательна для заполнения")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Фамилия должна быть от 2 до 100 символов")]
        [RegularExpression(@"^[A-Za-zА-ЯЁа-яё\s'-]+$",
      ErrorMessage = "Фамилия может содержать только буквы (русские и латинские) и апостроф")]
        [Display(Name = "Фамилия")]
        public string? LastName { get; set; }

       
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Отчество должно быть от 2 до 100 символов")]
        [RegularExpression(@"^[A-Za-zА-ЯЁа-яё\s'-]+$",
      ErrorMessage = "Отчество может содержать только буквы (русские и латинские) и апостроф")]
        [Display(Name = "Отчество")]
        public string? FatherName { get; set; }

        public string? Fio { get; set; }

        [Required(ErrorMessage = "Должность обязательна")]
        [StringLength(120, MinimumLength = 3, ErrorMessage = "Должность должна содержать от 3 до 120 символов")]
        [RegularExpression(@"^[A-Za-zА-ЯЁа-яё\s'()-]+$",
         ErrorMessage = "Должность может содержать буквы (русские и латинские), пробелы, дефис, апостроф и скобки")]
        [Display(Name = "Должность")]
        public string? Position { get; set; }

        [EmailAddress(ErrorMessage = "Введите корректный адрес электронной почты")]
        [Display(Name = "Email")]
        public string? Email { get; set; }
        public string? Author { get; set; }

        [RegularExpression(@"^\+?375\d{9}$",
ErrorMessage = "Телефон должен быть в формате +375XXXXXXXXX (12 цифр)")]
        [Display(Name = "Телефон")]
        public string? PhoneNumbers { get; set; }


        public bool IsActive { get; set; }

        public List<EmployeeContractDTO> EmployeeContracts { get; set; } = new List<EmployeeContractDTO>();

        public List<PhoneDTO> Phones { get; set; } = new List<PhoneDTO>();
        public virtual List<DepartmentEmployeeDTO> DepartmentEmployees { get; set; } = new List<DepartmentEmployeeDTO>();
    }
}
