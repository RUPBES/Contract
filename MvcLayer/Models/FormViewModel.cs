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

        [DisplayName("Стоимость ПНР")]
        public decimal? PnrCost { get; set; }

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

        [DisplayName("Номер")]
        public string Number { get; set; }

        [DisplayName("Собственными силами?")]
        public bool? IsOwnForces { get; set; }

        [DisplayName("Утвержденный вариант")]
        public bool? IsFinal { get; set; }

        public int? ContractId { get; set; }

        public string? OrganizationName { get; set; }

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
        [DisplayName("Сумма НДС для СМР")]
        public decimal? SmrNdsCost { get; set; }
        [DisplayName("Сумма НДС для дополнительных работ")]
        public decimal? AdditionalNdsCost { get; set; }



        public IFormFileCollection FilesEntity { get; set; }

        [DisplayName("Договор")]
        public virtual ContractDTO Contract { get; set; }
    }
}