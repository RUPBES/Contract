namespace BusinessLayer.Models
{
    public class ContractFileDTO
    {
        public int ContractId { get; set; }
        public int FileId { get; set; }

        public virtual ContractDTO Contract { get; set; }
        public virtual FileDTO File { get; set; }
    }
}
