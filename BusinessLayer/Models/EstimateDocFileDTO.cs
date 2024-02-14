namespace BusinessLayer.Models
{
    public class EstimateDocFileDTO
    {
        public int EstimateDocId { get; set; }
        public int FileId { get; set; }

        public virtual EstimateDocDTO EstimateDoc { get; set; }
        public virtual FileDTO File { get; set; }
    }
}
