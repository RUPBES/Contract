using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Models
{
    public class OrganizationDTO
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

        public List<AddressDTO> Addresses { get; set; } = new List<AddressDTO>();

        public List<ContractOrganizationDTO> ContractOrganizations { get; set; } = new List<ContractOrganizationDTO>();

        public List<DepartmentDTO> Departments { get; set; } = new List<DepartmentDTO>();

        public List<PhoneDTO> Phones { get; set; } = new List<PhoneDTO>();
    }
}
