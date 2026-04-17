namespace BusinessLayer.Models.KDO
{
    public class AdditionalTermDTO
    {
        public int Id { get; set; }

        public string? Number { get; set; }

        public DateTime? Date { get; set; }

        public DateTime? DueDate { get; set; }

        public string? Reason { get; set; }

        public bool? IsClaimLitigation { get; set; }

        public string? Type { get; set; }

        public int? ContractId { get; set; }

        public virtual ContractDTO? Contract { get; set; }

        public virtual ICollection<FileDTO> Files { get; set; } = new List<FileDTO>();
    }
}