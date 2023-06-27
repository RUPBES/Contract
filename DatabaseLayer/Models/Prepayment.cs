using System;
using System.Collections.Generic;

#nullable disable

namespace DatabaseLayer.Models
{
    public partial class Prepayment
    {
        public Prepayment()
        {
            InverseChangePrepayment = new HashSet<Prepayment>();
            PrepaymentAmendments = new HashSet<PrepaymentAmendment>();
        }

        public int Id { get; set; }
        public decimal? CurrentValue { get; set; }
        public decimal? CurrentValueFact { get; set; }
        public decimal? TargetValue { get; set; }
        public decimal? TargetValueFact { get; set; }
        public decimal? WorkingOutValue { get; set; }
        public decimal? WorkingOutValueFact { get; set; }
        public DateTime? Period { get; set; }
        public int? ContractId { get; set; }
        public bool? IsChange { get; set; }
        public int? ChangePrepaymentId { get; set; }

        public virtual Prepayment ChangePrepayment { get; set; }
        public virtual Contract Contract { get; set; }
        public virtual ICollection<Prepayment> InverseChangePrepayment { get; set; }
        public virtual ICollection<PrepaymentAmendment> PrepaymentAmendments { get; set; }
    }
}
