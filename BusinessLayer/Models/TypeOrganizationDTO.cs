using DatabaseLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Models
{
    public class TypeOrganizationDTO
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public List<ContractOrganizationDTO> ContractOrganizations { get; set; } = new List<ContractOrganizationDTO>();
    }
}
