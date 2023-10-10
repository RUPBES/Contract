using BusinessLayer.Models;
using System.ComponentModel;

namespace MvcLayer.Models
{
    public class ScopeWorkViewModel
    {
        public int Id { get; set; }

        [DisplayName("Период отчета")]
        public DateTime? Period { get; set; }

        [DisplayName("Cтоимость работ без НДС")]
        public decimal? CostNoNds { get; set; }

        [DisplayName("Cтоимость работ с НДС")]
        public decimal? CostNds { get; set; }

        [DisplayName("Стоимость СМР")]
        public decimal? SmrCost { get; set; }

        [DisplayName("Цена ПНР")]
        public decimal? PnrCost { get; set; }

        [DisplayName("Цена оборудования")]
        public decimal? EquipmentCost { get; set; }

        [DisplayName("Цена остальных работ")]
        public decimal? OtherExpensesCost { get; set; }

        [DisplayName("Цена доп. работ")]
        public decimal? AdditionalCost { get; set; }

        [DisplayName("Цена материалов")]
        public decimal? MaterialCost { get; set; }

        [DisplayName("Сумма услуг генподрядчика")]
        public decimal? GenServiceCost { get; set; }

        [DisplayName("Работы проводятся собственными силами?")]
        public bool? IsOwnForces { get; set; }

        [DisplayName("ID Контракт")]
        public int? ContractId { get; set; }

        [DisplayName("ID Изменений")]
        public int? AmendmentId { get; set; }

        [DisplayName("Изменено?")]
        public bool? IsChange { get; set; }

        [DisplayName("ID измененного объема работ")]
        public int? ChangeScopeWorkId { get; set; }

        public virtual ScopeWorkViewModel ChangeScopeWork { get; set; }
        public virtual ContractViewModel Contract { get; set; }
        public virtual List<ScopeWorkViewModel> InverseChangeScopeWork { get; set; } = new List<ScopeWorkViewModel>();
        public virtual List<ScopeWorkAmendmentDTO> ScopeWorkAmendments { get; set; } = new List<ScopeWorkAmendmentDTO>();
        public virtual List<SWCostDTO> SWCosts { get; set; } = new List<SWCostDTO>();
    }
}
