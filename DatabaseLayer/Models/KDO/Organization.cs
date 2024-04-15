using System;
using System.Collections.Generic;

#nullable disable

namespace DatabaseLayer.Models.KDO
{
    public partial class Organization
    {
        public Organization()
        {
            Addresses = new HashSet<Address>();
            ContractOrganizations = new HashSet<ContractOrganization>();
            Departments = new HashSet<Department>();
            Phones = new HashSet<Phone>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Abbr { get; set; }
        public string Unp { get; set; }
        public string Email { get; set; }
        public string PaymentAccount { get; set; }

        public virtual ICollection<Address> Addresses { get; set; }
        public virtual ICollection<ContractOrganization> ContractOrganizations { get; set; }
        public virtual ICollection<Department> Departments { get; set; }
        public virtual ICollection<Phone> Phones { get; set; }
    }
}
