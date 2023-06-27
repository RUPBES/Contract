using System;
using System.Collections.Generic;

#nullable disable

namespace DatabaseLayer.Models
{
    public partial class ServiceGc
    {
        public ServiceGc()
        {
            InverseChangeService = new HashSet<ServiceGc>();
            ServiceAmendments = new HashSet<ServiceAmendment>();
        }

        public int Id { get; set; }
        public int? ServicePercent { get; set; }
        public DateTime? Period { get; set; }
        public decimal? Price { get; set; }
        public decimal? FactPrice { get; set; }
        public int? ContractId { get; set; }
        public bool? IsChange { get; set; }
        public int? ChangeServiceId { get; set; }

        public virtual ServiceGc ChangeService { get; set; }
        public virtual Contract Contract { get; set; }
        public virtual ICollection<ServiceGc> InverseChangeService { get; set; }
        public virtual ICollection<ServiceAmendment> ServiceAmendments { get; set; }
    }
}
