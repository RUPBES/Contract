using BusinessLayer.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace MvcLayer.Models.Reports
{
    public class GetPayableCashPaymentsViewModel
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
        [DisplayName("Фактическое выполнение по справке C-3A, в т.ч.")]
        public decimal? factWorkByC3A { get; set; }
        [DisplayName("СМР и Авансы")]
        public List<ItemPaymentDeviationReport>? listPayments { get; set; }
        [DisplayName("Тип контракта")]
        public string? typeContract { get; set; }
    }

    public class ItemPaymentDeviationReport
    {
        [DisplayName("Период")]
        public DateTime? period { get; set; }
        [DisplayName("к оплате")]
        public decimal? payment { get; set; }
        [DisplayName("из них на счет РУП 'БЭС-УКХ'")]
        public decimal? paymentRupBes { get; set; }        
    }
}
