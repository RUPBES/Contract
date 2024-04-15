using System;
using System.Collections.Generic;

#nullable disable

namespace DatabaseLayer.Models.KDO
{
    public partial class ServiceGc
    {
        public ServiceGc()
        {
            InverseChangeService = new HashSet<ServiceGc>();
            ServiceAmendments = new HashSet<ServiceAmendment>();
            ServiceCosts = new HashSet<ServiceCost>();
        }

        public int Id { get; set; }
        public int? ServicePercent { get; set; }

        public int? ContractId { get; set; }
        public bool? IsChange { get; set; }
        public bool? IsFact { get; set; }
        public int? ChangeServiceId { get; set; }

        public virtual ServiceGc ChangeService { get; set; }
        public virtual Contract Contract { get; set; }
        public virtual ICollection<ServiceGc> InverseChangeService { get; set; }
        public virtual ICollection<ServiceAmendment> ServiceAmendments { get; set; }
        public virtual ICollection<ServiceCost> ServiceCosts { get; set; }
    }
}
