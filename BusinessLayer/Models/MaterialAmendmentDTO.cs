namespace BusinessLayer.Models
{
    public class MaterialAmendmentDTO
    {
        public int MaterialId { get; set; }
        public int AmendmentId { get; set; }

        public AmendmentDTO Amendment { get; set; }
        public MaterialDTO Material { get; set; }
    }
}
