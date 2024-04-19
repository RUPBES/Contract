using System.ComponentModel;

namespace MvcLayer.Models.Data
{
    public class PrepaymentPlanViewModel
    {
        public int Id { get; set; }

        public int? PrepaymentId { get; set; }

        [DisplayName("Текущие авансы")]
        public decimal? CurrentValue { get; set; }

        [DisplayName("Целевые Авансы")]
        public decimal? TargetValue { get; set; }

        [DisplayName("Отработка целевых")]
        public decimal? WorkingOutValue { get; set; }

        [DisplayName("Месяц за который получено")]
        public DateTime? Period { get; set; }
    }
}
