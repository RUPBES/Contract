using System.ComponentModel.DataAnnotations.Schema;

namespace DatabaseLayer.Models.KDO
{
    public class SWCost
    {
        public int Id { get; set; }
        public DateTime? Period { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public decimal? CostNoNds { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public decimal? CostNds { get; set; }
        public decimal? SmrCost { get; set; }
        public decimal? PnrCost { get; set; }
        public decimal? EquipmentCost { get; set; }
        public decimal? OtherExpensesCost { get; set; }
        public decimal? AdditionalCost { get; set; }
        public decimal? MaterialCost { get; set; }
        public decimal? GenServiceCost { get; set; }
        public bool? IsOwnForces { get; set; }
        public int? ScopeWorkId { get; set; }
        public virtual ScopeWork? ScopeWork { get; set; }
    }
}