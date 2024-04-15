namespace DatabaseLayer.Models.KDO
{
    public class ServiceCost
    {
        public int Id { get; set; }
        public DateTime? Period { get; set; }
        public decimal? Price { get; set; }
        public bool IsFact { get; set; }
        public int? ServiceGCId { get; set; }
        public virtual ServiceGc? ServiceGC { get; set; }
    }
}