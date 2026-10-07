
namespace BusinessLayer.Models.KDO
{
    public class PrepaymentReceivedDTO
    {
        public int Id { get; set; }

        public DateTime? Period { get; set; }

        public decimal? CurrentValue { get; set; }

        public decimal? TargetValue { get; set; }

        public int? ContractId { get; set; }

        public virtual ContractDTO? Contract { get; set; }
    }
}
