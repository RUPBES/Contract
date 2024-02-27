using DatabaseLayer.Models;

namespace BusinessLayer.Models
{
    public class PrepaymentDTO
    {
        public int Id { get; set; }     

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
