namespace BusinessLayer.Models
{
    public class ServiceGCDTO
    {
        public int Id { get; set; }
        public int? ServicePercent { get; set; }        
        public int? ContractId { get; set; }
        public bool? IsChange { get; set; }
        public bool? IsFact { get; set; }
        public int? ChangeServiceId { get; set; }

        public ServiceGCDTO ChangeService { get; set; }
        public ContractDTO Contract { get; set; }
        public List<ServiceGCDTO> InverseChangeService { get; set; } = new List<ServiceGCDTO>();
        public List<ServiceAmendmentDTO> ServiceAmendments { get; set; } = new List<ServiceAmendmentDTO>();
        public List<ServiceCostDTO> ServiceCosts { get; set; } = new List<ServiceCostDTO>();
    }
}
