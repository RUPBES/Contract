using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Models
{
    public class VContractDTO
    {
        public int Id { get; set; }

        public string? Number { get; set; }

        public string? ProcedureName { get; set; }

        public string? SignatoryEmp { get; set; }

        public string? ResponsibleEmp { get; set; }

        public string? GenContractor { get; set; }

        public string? Client { get; set; }
        public bool IsEngineering { get; set; }

        public int? SubContractId { get; set; }

        public int? AgreementContractId { get; set; }

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

        public double? PaymentСonditionsPrice { get; set; }
        public string? Author { get; set; }
        public string? Owner { get; set; }
        public bool IsExpired { get; set; }
        public bool IsClosed { get; set; }
        public decimal? ThisYearSum { get; set; }
        public decimal? PreYearSum { get; set; }
        public decimal? RemainingSum { get; set; }
    }
}
