namespace BusinessLayer.Models
{
    public class AmendmentFileDTO
    {
        public int AmendmentId { get; set; }
        public int FileId { get; set; }

        public AmendmentDTO Amendment { get; set; }
        public FileDTO File { get; set; }
    }
}
