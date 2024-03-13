using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Авансовые платежи получены
/// </summary>
namespace DatabaseLayer.Models
{
    public partial class PrepaymentTake
    {
        public int Id { get; set; }

        public string? Number { get; set; }

        public DateTime? Period { get; set; }

        public DateTime? DateTransfer { get; set; }

        public decimal? Total { get; set; }

        public Boolean? IsTarget { get; set; }

        public Boolean? IsRefund { get; set; }

        public int? PrepaymentId { get; set; }

        public virtual Prepayment? Prepayment { get; set; }

        public int? FileId { get; set; }

        public virtual File? File { get; set; }
    }
}
