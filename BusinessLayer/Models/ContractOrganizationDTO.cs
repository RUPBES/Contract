using DatabaseLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Models
{
    public class ContractOrganizationDTO
    {
        public int OrganizationId { get; set; }
        public int ContractId { get; set; }
        public bool? IsGenContractor { get; set; }
        public bool? IsClient { get; set; }

        public virtual ContractDTO Contract { get; set; }
        public virtual OrganizationDTO Organization { get; set; }
    }
}
