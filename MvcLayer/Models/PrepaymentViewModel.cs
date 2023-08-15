using BusinessLayer.Models;
using System.ComponentModel;

namespace MvcLayer.Models
{
    public class PrepaymentViewModel
    {
        public int Id { get; set; }       

        [DisplayName("Текущие авансы")]
        public decimal? CurrentValue { get; set; }

        [DisplayName("Текущие авансы по факту")]
        public decimal? CurrentValueFact { get; set; }

        [DisplayName("Целевые Авансы")]
        public decimal? TargetValue { get; set; }

        [DisplayName("Целевые Авансы по факту")]
        public decimal? TargetValueFact { get; set; }

        [DisplayName("Отработка целевых")]
        public decimal? WorkingOutValue { get; set; }

        [DisplayName("Отработка целевых фактическое")]
        public decimal? WorkingOutValueFact { get; set; }

        [DisplayName("Месяц за который получено")]
        public DateTime? Period { get; set; }

        [DisplayName("Изменено?")]
        public bool? IsChange { get; set; }

        [DisplayName("ID Контракт")]
        public int? ContractId { get; set; }

        [DisplayName("ID Изменений")]
        public int? AmendmentId { get; set; }

        [DisplayName("ID измененного аванса")]
        public int? ChangePrepaymentId { get; set; }

        public virtual PrepaymentViewModel Prepayment { get; set; }
        public virtual ContractViewModel Contract { get; set; }
        public virtual List<PrepaymentViewModel> InverseChangePrepayment { get; set; } = new List<PrepaymentViewModel>();
        public virtual List<PrepaymentAmendmentDTO> PrepaymentAmendments { get; set; } = new List<PrepaymentAmendmentDTO>();
    }
}
