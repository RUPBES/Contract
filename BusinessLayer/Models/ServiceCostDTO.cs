namespace BusinessLayer.Models
{
    public class ServiceCostDTO
    {
        public int Id { get; set; }
        public DateTime? Period { get; set; }
        public decimal? Price { get; set; }       
        public bool? IsFact { get; set; }
        public int? ServiceGCId { get; set; }
        public virtual ServiceGCDTO? ServiceGC { get; set; }
    }
}