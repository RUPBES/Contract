namespace MvcLayer.Models
{
    public class PaymentCashViewModel
    {
        public int Id { get; set; }

        public string? Number { get; set; }

        public string? GenContractor { get; set; }

        public string? Client { get; set; }

        public DateTime? Date { get; set; }

        public DateTime? EnteringTerm { get; set; }

        public DateTime? ContractTerm { get; set; }

        public DateTime? DateBeginWork { get; set; }

        public DateTime? DateEndWork { get; set; }

        public string? Сurrency { get; set; }

        public decimal? ContractPrice { get; set; }

        public string? NameObject { get; set; }

        public string? Author { get; set; }
        public string? Owner { get; set; }

        public decimal? ThisYearSum { get; set; }
        public decimal? FactSum { get; set; }
        public decimal? ReserveSum { get; set; }

    }
}

