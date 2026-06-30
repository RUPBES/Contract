using BusinessLayer.Models.KDO;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MvcLayer.Models
{
    public class OrganizationViewModel
    {
        public int Id { get; set; }

        /// <summary>
        /// Полное название
        /// </summary>
        [Required(ErrorMessage = "Обязательно для заполнения")]
        [StringLength(500, MinimumLength = 2, ErrorMessage = "Название должно содержать от 2 до 500 символов")]
        [Display(Name = "Полное название")]
        public string? Name { get; set; }

        /// <summary>
        /// Аббревиатура
        /// </summary>
        [Required(ErrorMessage = "Обязательно для заполнения")]
        [StringLength(500, MinimumLength = 2, ErrorMessage = "Название должно содержать от 2 до 500 символов")]
        [Display(Name = "Аббревиатура")]
        public string? Abbr { get; set; }

        /// <summary>
        /// УНП предприятия
        /// </summary>
        //[Required(ErrorMessage = "Обязательно для заполнения")]
        [RegularExpression(@"^\d{9}$", ErrorMessage = "УНП должен состоять из 9 цифр")]
        [StringLength(9, MinimumLength = 9, ErrorMessage = "УНП должен содержать ровно 9 символов")]
        [Display(Name = "УНП Организации")]
        public string? Unp { get; set; }

        /// <summary>
        /// электронная почта
        /// </summary>
        [DisplayName("Электронная почта")]
        [EmailAddress(ErrorMessage = "Введите корректный адрес электронной почты")]
        public string? Email { get; set; }

        /// <summary>
        /// расчетный счет
        /// </summary
        [DisplayName("Расчетный счет")]
        [RegularExpression(@"^BY\d{2}[A-Z0-9]{4}\d{4}[A-Z0-9]{16}$",
    ErrorMessage = "Расчётный счёт должен быть в формате IBAN (28 символов)")]
        //[Required(ErrorMessage = "Необходимо заполнить расчетный счет")]
        public string? PaymentAccount { get; set; }

        public List<AddressViewModel> Addresses { get; set; } = new List<AddressViewModel>();

        public List<ContractOrganizationDTO> ContractOrganizations { get; set; } = new List<ContractOrganizationDTO>();

        [DisplayName("Отделы")]
        public List<DepartmentViewModel> Departments { get; set; } = new List<DepartmentViewModel>();

        [DisplayName("Телефоны")]
        public List<PhoneViewModel> Phones { get; set; } = new List<PhoneViewModel>();
    }
}
