namespace BusinessLayer.Models.PRO
{
    public class EstimateFileDTO
    {
        public int EstimateId { get; set; }
        public int FileId { get; set; }

        public virtual EstimateDTO Estimate { get; set; }
        public virtual FileDTO File { get; set; }
    }
}