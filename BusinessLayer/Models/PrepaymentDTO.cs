using DatabaseLayer.Models;

namespace BusinessLayer.Models
{
    public class PrepaymentDTO
    {
        public int Id { get; set; }
        public decimal? CurrentValue { get; set; }
        public decimal? CurrentValueFact { get; set; }
        public decimal? TargetValue { get; set; }
        public decimal? TargetValueFact { get; set; }
        public decimal? WorkingOutValue { get; set; }
        public decimal? WorkingOutValueFact { get; set; }
        public DateTime? Period { get; set; }
        public int? ContractId { get; set; }
        public bool? IsChange { get; set; }
        public int? ChangePrepaymentId { get; set; }

        public PrepaymentDTO ChangePrepayment { get; set; }
        public ContractDTO Contract { get; set; }
        public List<PrepaymentDTO> InverseChangePrepayment { get; set; } = new List<PrepaymentDTO>();
        public List<PrepaymentAmendmentDTO> PrepaymentAmendments { get; set; } = new List<PrepaymentAmendmentDTO>();

        public List<PrepaymentFactDTO> PrepaymentFacts { get; set; } = new List<PrepaymentFactDTO>();

        public List<PrepaymentPlanDTO> PrepaymentPlans { get; set; } = new List<PrepaymentPlanDTO>();
    }
}
