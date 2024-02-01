using BusinessLayer.Models;
using System.ComponentModel;

namespace MvcLayer.Models
{
    public class PrepaymentStatementViewModel
    {
        [DisplayName("Объект")]
        public string? NameObject {get; set;}
        [DisplayName("Заказчик")]
        public string? Client {get; set;}
        [DisplayName("Целевого")]
        public decimal? TheoryTarget {get; set;}
        [DisplayName("Текущего")]
        public decimal? TheoryCurrent { get; set; }
        [DisplayName("Получено")]
        public decimal? TargetReceived { get; set; }
        [DisplayName("Погашено")]
        public decimal? TargetRepaid { get; set; }
        [DisplayName("Документ")]
        public string? NameAmendment { get; set; }
        [DisplayName("Файлы")]
        public List<FileDTO>? listFiles { get; set; }
        [DisplayName("СМР и Авансы")]
        public List<SmrWithPrepayment>? listSmrWithAvans { get; set; }
    }
    public class SmrWithPrepayment
    {
        [DisplayName("Месяц за который получено")]
        public DateTime? Period { get; set; }
        [DisplayName("Целевой план")]
        public decimal? TargetPlan { get; set; }
        [DisplayName("Целевой факт")]
        public decimal? TargetFact { get; set; }
        [DisplayName("Текущий план")]
        public decimal? CurrentPlan { get; set; }
        [DisplayName("Текущий факт")]
        public decimal? CurrentFact { get; set; }
        [DisplayName("СМР факт")]
        public decimal? SmrFact { get; set; }
        [DisplayName("СМР план")]
        public decimal? SmrPlan { get; set; }
    }
}
