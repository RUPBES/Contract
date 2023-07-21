namespace BusinessLayer.Models
{
    public class ServiceGCDTO
    {
        public int Id { get; set; }
        public int? ServicePercent { get; set; }
        public DateTime? Period { get; set; }
        public decimal? Price { get; set; }
        public decimal? FactPrice { get; set; }
        public int? ContractId { get; set; }
        public bool? IsChange { get; set; }
        public int? ChangeServiceId { get; set; }

        public ServiceGCDTO ChangeService { get; set; }
        public ContractDTO Contract { get; set; }
        public List<ServiceGCDTO> InverseChangeService { get; set; }
        public List<ServiceAmendmentDTO> ServiceAmendments { get; set; }
    }
}
