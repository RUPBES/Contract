
namespace BusinessLayer.Models
{
    public class PrepaymentPlanDTO
    {
        public int Id { get; set; }

        public DateTime? Period { get; set; }

        public decimal? CurrentValue { get; set; }

        public decimal? TargetValue { get; set; }

        public decimal? WorkingOutValue { get; set; }

        public int? PrepaymentId { get; set; }

        public virtual PrepaymentDTO? Prepayment { get; set; }
    }
}
