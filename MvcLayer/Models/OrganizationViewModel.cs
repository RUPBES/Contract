using BusinessLayer.Models;

namespace MvcLayer.Models
{
    public class OrganizationViewModel
    {

        public int Id { get; set; }

        /// <summary>
        /// Полное название
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Аббревиатура
        /// </summary>
        public string? Abbr { get; set; }

        public string? Unp { get; set; }

        public List<AddressViewModel> Addresses { get; set; } = new List<AddressViewModel>();

        public List<ContractOrganizationDTO> ContractOrganizations { get; set; } = new List<ContractOrganizationDTO>();

        public List<DepartmentViewModel> Departments { get; set; } = new List<DepartmentViewModel>();

        public List<PhoneViewModel> Phones { get; set; } = new List<PhoneViewModel>();
    }
}
