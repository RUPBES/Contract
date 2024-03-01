using BusinessLayer.Models;
using DatabaseLayer.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace MvcLayer.Models.Reports
{
    public class GetCostDeviationScopeWorkViewModel
    {
        [DisplayName("Идентификатор")]
        public int Id { get; set; }
        [DisplayName("Дата договора")]
        public DateTime? dateContract { get; set; }
        [DisplayName("Номер договора")]
        public string? number { get; set; }
        [DisplayName("Название объекта")]
        public string? nameObject { get; set; }
        [DisplayName("Генподрядчик")]
        public string? genContractor { get; set; }
        [DisplayName("Cубподрядчик")]
        public string? subContractor { get; set; }
        [DisplayName("Заказчик")]
        public string? client { get; set; }
        [DisplayName("Дата начала работ")]
        public DateTime? dateBeginWork { get; set; }
        [DisplayName("Дата конца работ")]
        public DateTime? dateEndWork { get; set; }
        [DisplayName("Дата ввода объекта")]
        public DateTime? dateEnter { get; set; }
        [DisplayName("Валюта")]
        public string? currency { get; set; }
        [DisplayName("Контрактная цена")]
        public decimal? contractPrice { get; set; }
        [DisplayName("Остаток по выполнению")]
        public decimal? remainingWork { get; set; }
        [DisplayName("Объем на текущий год")]
        public decimal? currentYearScopeWork { get; set; }
        [DisplayName("СМР и Авансы")]
        public List<ItemScopeDeviationReport>? listScopeWork { get; set; }
        [DisplayName("Тип контракта")]
        public string? typeContract { get; set; }
    }

    public class ItemScopeDeviationReport
    {
        [DisplayName("Период")]
        public DateTime? period { get; set; }
        [DisplayName("Дата начала работ")]
        public ScopeWorkForReport? planScopeWork { get; set; }
        [DisplayName("Дата начала работ")]
        public ScopeWorkForReport? factScopeWork { get; set; }
    }

    public class ScopeWorkForReport             
    {
        public DateTime? Period { get; set; }
        public decimal? SmrCost { get; set; }
        public decimal? PnrCost { get; set; }
        public decimal? EquipmentCost { get; set; }
        public decimal? OtherExpensesCost { get; set; }
        public decimal? AdditionalCost { get; set; }
        public decimal? MaterialCost { get; set; }

        public ScopeWorkForReport(DateTime? period, decimal? smrCost, decimal? pnrCost, decimal? equipmentCost, decimal? otherExpensesCost, decimal? additionalCost, decimal? materialCost)
        {
            Period = period;
            SmrCost = smrCost;
            PnrCost = pnrCost;
            EquipmentCost = equipmentCost;
            OtherExpensesCost = otherExpensesCost;
            AdditionalCost = additionalCost;
            MaterialCost = materialCost;
        }

        public ScopeWorkForReport()
        {            
            SmrCost = 0;
            PnrCost = 0;
            EquipmentCost = 0;
            OtherExpensesCost = 0;
            AdditionalCost = 0;
            MaterialCost = 0;
        }
    }

}
