using System;
using System.Collections.Generic;

#nullable disable

namespace DatabaseLayer.Models
{
    public partial class ScopeWork
    {
        public ScopeWork()
        {
            InverseChangeScopeWork = new HashSet<ScopeWork>();
            ScopeWorkAmendments = new HashSet<ScopeWorkAmendment>();
        }

        public int Id { get; set; }
        public DateTime? Period { get; set; }
        public decimal? CostNoNds { get; set; }
        public decimal? CostNds { get; set; }
        public decimal? SmrCost { get; set; }
        public decimal? PnrCost { get; set; }
        public decimal? EquipmentCost { get; set; }
        public decimal? OtherExpensesCost { get; set; }
        public decimal? AdditionalCost { get; set; }
        public decimal? MaterialCost { get; set; }
        public decimal? GenServiceCost { get; set; }
        public bool? IsOwnForces { get; set; }
        public int? ContractId { get; set; }
        public bool? IsChange { get; set; }
        public int? ChangeScopeWorkId { get; set; }

        public virtual ScopeWork ChangeScopeWork { get; set; }
        public virtual Contract Contract { get; set; }
        public virtual ICollection<ScopeWork> InverseChangeScopeWork { get; set; }
        public virtual ICollection<ScopeWorkAmendment> ScopeWorkAmendments { get; set; }
    }
}
