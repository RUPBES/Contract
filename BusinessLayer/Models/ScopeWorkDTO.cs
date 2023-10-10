using DatabaseLayer.Models;

namespace BusinessLayer.Models
{
    public class ScopeWorkDTO
    {
        public int Id { get; set; }
        //public DateTime? Period { get; set; }
        //public decimal? CostNoNds { get; set; }
        //public decimal? CostNds { get; set; }
        //public decimal? SmrCost { get; set; }
        //public decimal? PnrCost { get; set; }
        //public decimal? EquipmentCost { get; set; }
        //public decimal? OtherExpensesCost { get; set; }
        //public decimal? AdditionalCost { get; set; }
        //public decimal? MaterialCost { get; set; }
        //public decimal? GenServiceCost { get; set; }
        public bool? IsOwnForces { get; set; }
        public int? ContractId { get; set; }
        public bool? IsChange { get; set; }
        public int? ChangeScopeWorkId { get; set; }

        public virtual ScopeWorkDTO ChangeScopeWork { get; set; }
        public virtual ContractDTO Contract { get; set; }
        public virtual List<ScopeWorkDTO> InverseChangeScopeWork { get; set; } = new List<ScopeWorkDTO>();
        public virtual List<ScopeWorkAmendmentDTO> ScopeWorkAmendments { get; set; } = new List<ScopeWorkAmendmentDTO>();
        public virtual List<SWCost> SWCosts { get; set; } = new List<SWCost>();
    }
}
