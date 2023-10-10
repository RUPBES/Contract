using BusinessLayer.Models;
using System.ComponentModel;

namespace MvcLayer.Models
{
    public class PeriodChooseViewModel
    {

        [DisplayName("Начало работ")]
        public DateTime PeriodStart { get; set; }
        [DisplayName("Окончание работ")]
        public DateTime PeriodEnd { get; set; }

        [DisplayName("ID Контракт")]
        public int? ContractId { get; set; }

        [DisplayName("Изменено?")]
        public bool? IsChange { get; set; }

        [DisplayName("Собственными силами?")]
        public bool IsOwnForces { get; set; }

        [DisplayName("ID измененного объема работ")]
        public int? ChangeScopeWorkId { get; set; }
        [DisplayName("ID измененного аванса")]
        public int? ChangePrepaymentId { get; set; }
        [DisplayName("ID измененного услуг")]
        public int? ChangeServiceId { get; set; }
        [DisplayName("ID измененного материалов")]
        public int? ChangeMaterialId { get; set; }

        [DisplayName("ID изменений к договору")]
        public int? AmendmentId { get; set; }

        [DisplayName("Собственными силами?")]
        public bool IsFact { get; set; }

        public DateTime? ChoosePeriod { get; set; }
        public List<DateTime> ListDates { get; set; } = new List<DateTime>();
    }
}
