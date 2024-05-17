using BusinessLayer.Models;
using System.ComponentModel;

namespace MvcLayer.Models.Reports
{
    public class PrepaymentStatementViewModel
    {
        [DisplayName("Период начала")]
        public DateTime? startPeriod { get; set; }
        [DisplayName("Период окончания")]
        public DateTime? endPeriod { get; set; }
        [DisplayName("Период начала")]
        public DateTime? minStartPeriod { get; set; }
        [DisplayName("Период окончания")]
        public DateTime? maxEndPeriod { get; set; }
        [DisplayName("Объект")]
        public string? NameObject { get; set; }
        [DisplayName("Заказчик")]
        public string? Client { get; set; }

        [DisplayName("Целевого")]
        public decimal? TheoryTarget { get; set; }

        [DisplayName("Текущего")]
        public decimal? TheoryCurrent { get; set; }

        [DisplayName("Получено")]
        public decimal? TargetReceived { get; set; }

        [DisplayName("Погашено")]
        public decimal? TargetRepaid { get; set; }

        [DisplayName("Документ")]
        public string? NameAmendment { get; set; }
        [DisplayName("Файлы")]
        public List<FileWithDate>? listFiles { get; set; }
        [DisplayName("СМР и Авансы")]
        public List<SmrWithPrepayment>? listSmrWithAvans { get; set; }
    }

    public class FileWithDate
    {
        public IEnumerable<FileDTO>? file { get; set; }
        public DateTime? date { get; set; }
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

    public class PrepaymentStatementWithAmendmentViewModel
    {
        [DisplayName("Период начала")]
        public DateTime? startPeriod { get; set; }
        [DisplayName("Период окончания")]
        public DateTime? endPeriod { get; set; }
        [DisplayName("Период начала")]
        public DateTime? minStartPeriod { get; set; }
        [DisplayName("Период окончания")]
        public DateTime? maxEndPeriod { get; set; }
        [DisplayName("Объект")]
        public string? NameObject { get; set; }
        [DisplayName("Заказчик")]
        public string? Client { get; set; }
        [DisplayName("Целевого")]
        public decimal? TheoryTarget { get; set; }
        [DisplayName("Текущего")]
        public decimal? TheoryCurrent { get; set; }
        [DisplayName("Получено")]
        public decimal? TargetReceived { get; set; }
        [DisplayName("Погашено")]
        public decimal? TargetRepaid { get; set; }
        [DisplayName("Файлы")]
        public List<FileWithDate>? listFiles { get; set; }
        [DisplayName("СМР и Авансы")]
        public List<ListSmrPrepByAmendment>? listSmrWithPrepaymentByAmendment { get; set; }
    }

    public class ListSmrPrepByAmendment
    {
        [DisplayName("СМР и Авансы")]
        public List<ElementOfListSmrPrepByAmend>? listSmrWithAvans { get; set; }
        [DisplayName("Документ")]
        public string? NameAmendment { get; set; }
    }

    public class ElementOfListSmrPrepByAmend
    {
        [DisplayName("Месяц за который получено")]
        public DateTime? Period { get; set; }
        [DisplayName("Целевой")]
        public decimal? Target { get; set; }
        [DisplayName("Текущий")]
        public decimal? Current { get; set; }
        [DisplayName("СМР")]
        public decimal? Smr { get; set; }
    }
}
