using BusinessLayer.Models;
using System.ComponentModel;

namespace MvcLayer.Models
{
    public class OrganizationViewModel
    {
        public int Id { get; set; }

        /// <summary>
        /// Полное название
        /// </summary>
        [DisplayName("Полное название организации")]
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
        public string? Unp { get; set; }

        /// <summary>
        /// электронная почта
        /// </summary>
        [DisplayName("Электронная почта")]
        public string? Email { get; set; }

        /// <summary>
        /// расчетный счет
        /// </summary>
        [DisplayName("Расчетный счет")]
        public string? PaymentAccount { get; set; }

        public List<AddressViewModel> Addresses { get; set; } = new List<AddressViewModel>();

        public List<ContractOrganizationDTO> ContractOrganizations { get; set; } = new List<ContractOrganizationDTO>();

        public List<DepartmentViewModel> Departments { get; set; } = new List<DepartmentViewModel>();

        public List<PhoneViewModel> Phones { get; set; } = new List<PhoneViewModel>();
    }
}
