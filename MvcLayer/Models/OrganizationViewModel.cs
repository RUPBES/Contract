using BusinessLayer.Models;
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
        [DisplayName("Полное название организации")]
        [Required(ErrorMessage = "Заполните полное название организации")]
        public string? Name { get; set; }

        /// <summary>
        /// Аббревиатура
        /// </summary>
        [DisplayName("Аббревиатура")]
        public string? Abbr { get; set; }

        /// <summary>
        /// УНП предприятия
        /// </summary>
        [DisplayName("УНП организации")]
        [RegularExpression("^[ 0-9]+$", ErrorMessage = "Только цифры")]
        [StringLength(15, MinimumLength = 9, ErrorMessage = "Длина строки должна быть от 9 до 15 символов")]
        [Required(ErrorMessage = "Необходимо заполнить УНП")]
        public string? Unp { get; set; }

        /// <summary>
        /// электронная почта
        /// </summary>
        [DisplayName("Электронная почта")]
        public string? Email { get; set; }

        /// <summary>
        /// расчетный счет
        /// </summary
        [DisplayName("Расчетный счет")]
        [RegularExpression("[A-Z]{2}[0-9]{2}[A-Z0-9]{4}[0-9]{4}([A-Z0-9]?){16}", ErrorMessage = "28 - разрядов; 1,2 - буквы;  3,4,9-12  - цифры; 5-8,13-28 латиница и цифры")]
        [Required(ErrorMessage = "Необходимо заполнить расчетный счет")]
        public string? PaymentAccount { get; set; }

        public List<AddressViewModel> Addresses { get; set; } = new List<AddressViewModel>();

        public List<ContractOrganizationDTO> ContractOrganizations { get; set; } = new List<ContractOrganizationDTO>();

        public List<DepartmentViewModel> Departments { get; set; } = new List<DepartmentViewModel>();

        public List<PhoneViewModel> Phones { get; set; } = new List<PhoneViewModel>();
    }
}
