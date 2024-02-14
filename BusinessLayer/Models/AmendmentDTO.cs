using DatabaseLayer.Models;

namespace BusinessLayer.Models
{
    public class AmendmentDTO
    {
        public int Id { get; set; }
        public string Number { get; set; }
        public DateTime? Date { get; set; }
        public string Reason { get; set; }
        public decimal? ContractPrice { get; set; }
        public DateTime? DateBeginWork { get; set; }
        public DateTime? DateEndWork { get; set; }
        public DateTime? DateEntryObject { get; set; }
        public string ContractChanges { get; set; }
        public string Comment { get; set; }
        public int? ContractId { get; set; }

        public ContractDTO Contract { get; set; }
        public List<AmendmentFileDTO> AmendmentFiles { get; set; }
        public List<MaterialAmendmentDTO> MaterialAmendments { get; set; } = new List<MaterialAmendmentDTO>();
        public List<PrepaymentAmendmentDTO> PrepaymentAmendments { get; set; } = new List<PrepaymentAmendmentDTO>();
        public List<ScopeWorkAmendmentDTO> ScopeWorkAmendments { get; set; } = new List<ScopeWorkAmendmentDTO>();
        public List<ServiceAmendmentDTO> ServiceAmendments { get; set; } = new List<ServiceAmendmentDTO>();
    }
}
