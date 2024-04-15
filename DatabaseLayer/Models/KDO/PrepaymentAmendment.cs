using System;
using System.Collections.Generic;

#nullable disable

namespace DatabaseLayer.Models.KDO
{
    public partial class PrepaymentAmendment
    {
        public int PrepaymentId { get; set; }
        public int AmendmentId { get; set; }

        public virtual Amendment Amendment { get; set; }
        public virtual Prepayment Prepayment { get; set; }
    }
}
