using DatabaseLayer.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.Metrics;

namespace BusinessLayer.Models
{
    public class SWCostDTO
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
        public virtual ScopeWorkDTO? ScopeWork { get; set; }

        public SWCostDTO()
        {
            Id = 0;
            Period = null;
            CostNoNds = 0;
            CostNds = 0;
            SmrCost = 0;
            PnrCost = 0;
            EquipmentCost = 0;
            OtherExpensesCost = 0;
            AdditionalCost = 0;
            MaterialCost = 0;
            GenServiceCost = 0;
        }

        public static SWCostDTO operator +(SWCostDTO a, SWCost b) 
        {
            var result = new SWCostDTO();
            result.CostNds = a.CostNds + b.CostNds ;
            result.CostNoNds = a.CostNoNds + b.CostNoNds;
            result.SmrCost = a.SmrCost + b.SmrCost;
            result.PnrCost = a.PnrCost + b.PnrCost;
            result.AdditionalCost = a.AdditionalCost + b.AdditionalCost;
            result.EquipmentCost = a.EquipmentCost + b.EquipmentCost;
            result.OtherExpensesCost = a.OtherExpensesCost + b.OtherExpensesCost;
            result.MaterialCost = a.MaterialCost + b.MaterialCost;
            result.GenServiceCost = a.GenServiceCost + b.GenServiceCost;
            return result;
        }

        public static SWCostDTO operator +(SWCostDTO a, SWCostDTO b)
        {
            var result = new SWCostDTO();
            result.CostNds = a.CostNds + b.CostNds;
            result.CostNoNds = a.CostNoNds + b.CostNoNds;
            result.SmrCost = a.SmrCost + b.SmrCost;
            result.PnrCost = a.PnrCost + b.PnrCost;
            result.AdditionalCost = a.AdditionalCost + b.AdditionalCost;
            result.EquipmentCost = a.EquipmentCost + b.EquipmentCost;
            result.OtherExpensesCost = a.OtherExpensesCost + b.OtherExpensesCost;
            result.MaterialCost = a.MaterialCost + b.MaterialCost;
            result.GenServiceCost = a.GenServiceCost + b.GenServiceCost;
            return result;
        }

        public static SWCostDTO operator +(SWCost a, SWCostDTO b)
        {
            var result = new SWCostDTO();
            result.CostNds = a.CostNds + b.CostNds;
            result.CostNoNds = a.CostNoNds + b.CostNoNds;
            result.SmrCost = a.SmrCost + b.SmrCost;
            result.PnrCost = a.PnrCost + b.PnrCost;
            result.AdditionalCost = a.AdditionalCost + b.AdditionalCost;
            result.EquipmentCost = a.EquipmentCost + b.EquipmentCost;
            result.OtherExpensesCost = a.OtherExpensesCost + b.OtherExpensesCost;
            result.MaterialCost = a.MaterialCost + b.MaterialCost;
            result.GenServiceCost = a.GenServiceCost + b.GenServiceCost;
            return result;
        }

        public static SWCostDTO operator +(SWCostDTO a, FormDTO b)
        {            
            var result = new SWCostDTO();
            result.CostNds = a.CostNds + b.TotalCost;
            result.CostNoNds = a.CostNoNds + (b.TotalCost / (decimal)1.2);
            result.SmrCost = a.SmrCost + b.SmrCost;
            result.PnrCost = a.PnrCost + b.PnrCost;
            result.AdditionalCost = a.AdditionalCost + b.AdditionalCost;
            result.EquipmentCost = a.EquipmentCost + b.EquipmentCost;
            result.OtherExpensesCost = a.OtherExpensesCost + b.OtherExpensesCost;
            result.MaterialCost = a.MaterialCost + b.MaterialCost;
            result.GenServiceCost = a.GenServiceCost + b.GenServiceCost;
            return result;
        }
    }
}