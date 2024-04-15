using System;
using System.Collections.Generic;

#nullable disable

namespace DatabaseLayer.Models.KDO
{
    public partial class ScopeWork
    {
        public ScopeWork()
        {
            InverseChangeScopeWork = new HashSet<ScopeWork>();
            ScopeWorkAmendments = new HashSet<ScopeWorkAmendment>();
            SWCosts = new HashSet<SWCost>();
        }

        public int Id { get; set; }
        public int? ContractId { get; set; }
        public bool? IsChange { get; set; }
        public int? ChangeScopeWorkId { get; set; }
        public bool? IsOwnForces { get; set; }

        public virtual ScopeWork ChangeScopeWork { get; set; }
        public virtual Contract Contract { get; set; }
        public virtual ICollection<ScopeWork> InverseChangeScopeWork { get; set; }
        public virtual ICollection<ScopeWorkAmendment> ScopeWorkAmendments { get; set; }
        public virtual ICollection<SWCost> SWCosts { get; set; }
    }
}
