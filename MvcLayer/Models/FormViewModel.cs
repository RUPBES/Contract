using BusinessLayer.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MvcLayer.Models
{
    public class FormViewModel
    {
        public int Id { get; set; }

        [DisplayName("Период составления справки")]
        public DateTime? Period { get; set; }

        [DisplayName("Дата подписания")]
        public DateTime? DateSigning { get; set; }

        [DisplayName("Общая стоимость")]
        public decimal? TotalCost { get; set; }

        [DisplayName("Стоимость СМР")]
        public decimal? SmrCost { get; set; }

        [DisplayName("Неизменная договорная цена")]
        public decimal? FixedContractPrice { get; set; }

        [DisplayName("Договорная цена")]
        public decimal? PnrCost { get; set; }

        [DisplayName("Стоимость ПНР")]
        public decimal? PnrCostTotal { get; set; }

        [DisplayName("Стоимость оборудования")]
        public decimal? EquipmentCost { get; set; }

        [DisplayName("Стоимость прочих работ")]
        public decimal? OtherExpensesCost { get; set; }

        [DisplayName("Стоимость доп. работ")]
        public decimal? AdditionalCost { get; set; }

        [DisplayName("Материалы генподрядчика")]
        public decimal? MaterialCost { get; set; }

        [DisplayName("Стоимость ген.услуг")]
        public decimal? GenServiceCost { get; set; }

        [DisplayName("Зачет целевого аванса")]
        public decimal? OffsetTargetPrepayment { get; set; }
        [DisplayName("Зачет текущего аванса")]
        public decimal? OffsetCurrentPrepayment { get; set; }

        [DisplayName("К оплате")]
        public decimal? TotalCostToBePaid { get; set; }

        [DisplayName("Собственными силами?")]
        public bool? IsOwnForces { get; set; }

        [DisplayName("Утвержденный вариант")]
        public bool? IsFinal { get; set; }

        public int? ContractId { get; set; }    

        [DisplayName("Освобожден от уплаты ндс?")]
        public bool? IsExemptFromVAT { get; set; } //освобожден от уплаты ндс?
        [DisplayName("Стоимость материалов (заказчика)")]
        public decimal? MaterialClientCost { get; set; } //стоимость материалов (заказчика)
        [DisplayName("Стоимость оборудования заказчика (справочно)")]
        public decimal? EquipmentClientCost { get; set; } //стоимость оборудования заказчика (справочно)
        [DisplayName("Отчисления в фонд строительной отрасли")]
        public decimal? CostToConstructionIndustryFund { get; set; } //отчисления в фонд строительной отрасли
        [DisplayName("Стоимость работ для статистической отчетности подрядчика (слравочно)")]
        public decimal? СostStatisticReportOfContractor { get; set; } //стоимость работ для статистической отчетности подрядчика (слравочно)
        [DisplayName("Сумма НДС для договорной цены")]
        public decimal? SmrNdsCost { get; set; }
        [DisplayName("Сумма НДС для дополнительных работ")]
        public decimal? AdditionalNdsCost { get; set; }
        [DisplayName("Сумма НДС ПНР")]
        public decimal? PnrNdsCost { get; set; }
        [DisplayName("Сумма НДС для оборудования")]
        public decimal? EquipmentNdsCost { get; set; }

        public IFormFileCollection FilesEntity { get; set; }
    }
}