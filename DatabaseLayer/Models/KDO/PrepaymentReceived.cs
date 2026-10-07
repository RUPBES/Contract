namespace DatabaseLayer.Models.KDO
{
    public partial class PrepaymentReceived
    {
        public int Id { get; set; }

        public DateTime? Period { get; set; }

        public decimal? CurrentValue { get; set; }

        public decimal? TargetValue { get; set; }

        public int? ContractId { get; set; }

        public virtual Contract? Contract { get; set; }
    }
}
