namespace BusinessLayer.Models
{
    public class CommissionActFileDTO
    {
        public int СommissionActId { get; set; }
        public int FileId { get; set; }

        public FileDTO File { get; set; }
        public CommissionActDTO СommissionAct { get; set; }
    }
}
