using System;
using System.Collections.Generic;

#nullable disable

namespace DatabaseLayer.Models.KDO
{
    public partial class ContractOrganization
    {
        public int OrganizationId { get; set; }
        public int ContractId { get; set; }
        public bool? IsGenContractor { get; set; }
        public bool? IsClient { get; set; }
        public bool? IsResponsibleForWork { get; set; }

        public virtual Contract Contract { get; set; }
        public virtual Organization Organization { get; set; }
    }
}
