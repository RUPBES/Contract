﻿namespace DatabaseLayer.Models
{
    public class VContractEngin
    {
        public int Id { get; set; }

        public string? Number { get; set; }

        public string? ProcedureName { get; set; }

        public string? GenContractor { get; set; }

        public string? Client { get; set; }
        public bool? IsEngineering { get; set; }

        public DateTime? Date { get; set; }

        public DateTime? EnteringTerm { get; set; }

        public DateTime? ContractTerm { get; set; }

        public DateTime? DateBeginWork { get; set; }

        public DateTime? DateEndWork { get; set; }

        public string? Сurrency { get; set; }

        public decimal? ContractPrice { get; set; }

        public string? NameObject { get; set; }

        public string? FundingSource { get; set; }

        public string? PaymentСonditionsAvans { get; set; }

        public string? PaymentСonditionsRaschet { get; set; }
    }
}