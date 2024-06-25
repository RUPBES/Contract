using BusinessLayer.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MvcLayer.Models
{
    public class FormViewModel
    {
        public int Id { get; set; }
        public int? ContractId { get; set; }
        public IFormFileCollection FilesEntity { get; set; }

        [DisplayName("Собственными силами?")]
        public bool? IsOwnForces { get; set; }

        [DisplayName("Освобожден от уплаты ндс?")]
        public bool? IsExemptFromVAT { get; set; } //освобожден от уплаты ндс?

        [DisplayName("Период составления справки")]
        public DateTime? Period { get; set; }

        [DisplayName("Дата подписания")]
        public DateTime? DateSigning { get; set; }

        [DisplayName("Общая стоимость")]
        public decimal? TotalCost { get; set; }

        [DisplayName("К оплате")]
        public decimal? TotalCostToBePaid { get; set; }

        [DisplayName("Стоимость СМР")]
        public decimal? SmrCost { get; set; }
        [DisplayName("Неизменная договорная цена")]
        public decimal? SmrContractCost { get; set; }
        [DisplayName("Сумма НДС для договорной цены")]
        public decimal? SmrNdsCost { get; set; }

        [DisplayName("Стоимость доп. работ")]
        public decimal? AdditionalCost { get; set; }
        [DisplayName("Контрактная цена(без НДС) по дополнительным работам")]
        public decimal? AdditionalContractCost { get; set; }
        [DisplayName("Сумма НДС для дополнительных работ")]
        public decimal? AdditionalNdsCost { get; set; }

        [DisplayName("Стоимость ПНР")]
        public decimal? PnrCost { get; set; }
        [DisplayName("Контрактная цена(без НДС) по ПНР")]
        public decimal? PnrContractCost { get; set; }
        [DisplayName("Сумма НДС по ПНР")]
        public decimal? PnrNdsCost { get; set; }

        [DisplayName("Стоимость оборудования")]
        public decimal? EquipmentCost { get; set; }
        [DisplayName("Контрактная цена(без НДС) по оборудованию")]
        public decimal? EquipmentContractCost { get; set; }
        [DisplayName("Сумма НДС для оборудования")]
        public decimal? EquipmentNdsCost { get; set; }
        [DisplayName("Стоимость оборудования заказчика (справочно)")]
        public decimal? EquipmentClientCost { get; set; } //стоимость оборудования заказчика (справочно)

        [DisplayName("Стоимость прочих работ")]
        public decimal? OtherExpensesCost { get; set; }
        [DisplayName("Сумма НДС прочих работ")]
        public decimal? OtherExpensesNdsCost { get; set; }

        [DisplayName("Материалы генподрядчика")]
        public decimal? MaterialCost { get; set; }
        [DisplayName("Стоимость материалов заказчика (справочно)")]
        public decimal? MaterialClientCost { get; set; } //стоимость материалов (заказчика)

        [DisplayName("Стоимость ген.услуг")]
        public decimal? GenServiceCost { get; set; }

        [DisplayName("Зачет целевого аванса")]
        public decimal? OffsetTargetPrepayment { get; set; }
        [DisplayName("Зачет текущего аванса")]
        public decimal? OffsetCurrentPrepayment { get; set; }       
     
        [DisplayName("Отчисления в фонд строительной отрасли")]
        public decimal? CostToConstructionIndustryFund { get; set; } //отчисления в фонд строительной отрасли
        [DisplayName("Стоимость работ для статистической отчетности подрядчика (справочно)")]
        public decimal? CostStatisticReportOfContractor { get; set; } //стоимость работ для статистической отчетности подрядчика (слравочно)  
    }
}