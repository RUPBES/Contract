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

        public int ContactId { get; set; }

        public int? TypeOrgId { get; set; }

        public ContractDTO Contact { get; set; } = null!;

        public OrganizationDTO Organization { get; set; } = null!;

        public TypeOrganizationDTO? TypeOrg { get; set; }
    }
}
