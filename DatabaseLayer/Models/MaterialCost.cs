namespace DatabaseLayer.Models
{
    public class MaterialCost
    {
        public int Id { get; set; }
        public DateTime? Period { get; set; }
        public decimal? Price { get; set; }
        public bool IsFact { get; set; }
        public int? MaterialId { get; set; }
        public virtual MaterialGc? Material { get; set; }
    }
}