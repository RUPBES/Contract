using BusinessLayer.Models;
using System.ComponentModel;

namespace MvcLayer.Models
{
    public class ScopeWorkViewModel
    {
        public int Id { get; set; }

        [DisplayName("Период отчета")]
        public DateTime? Period { get; set; }

        [DisplayName("Всего по договору без НДС (справочно)")]
        public decimal? CostNoNds { get; set; }

        [DisplayName("Всего по договору с НДС")]
        public decimal? CostNds { get; set; }

        [DisplayName("Неизменная цена СМР")]
        public decimal? SmrCost { get; set; }

        [DisplayName("ПНР")]
        public decimal? PnrCost { get; set; }

        [DisplayName("Оборудование генподрядчика")]
        public decimal? EquipmentCost { get; set; }

        [DisplayName("Прочие работы и услуги")]
        public decimal? OtherExpensesCost { get; set; }

        [DisplayName("Доп. работы")]
        public decimal? AdditionalCost { get; set; }

        [DisplayName("Материалы генподрядчика")]
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
