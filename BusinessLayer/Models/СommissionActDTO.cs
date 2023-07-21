namespace BusinessLayer.Models
{
    public class CommissionActDTO
    {
        public int Id { get; set; }
        public string Number { get; set; }
        public DateTime? Date { get; set; }
        public int? ContractId { get; set; }

        public ContractDTO Contract { get; set; }
        public List<CommissionActFileDTO> СommissionActFiles { get; set; } = new List<CommissionActFileDTO>();
    }
}
