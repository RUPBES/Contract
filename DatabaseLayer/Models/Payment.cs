using System;
using System.Collections.Generic;

#nullable disable

namespace DatabaseLayer.Models
{
    public partial class Payment
    {
        public int Id { get; set; }
        public decimal? PaySum { get; set; }
        public decimal? PaySumForRupBes { get; set; }
        public DateTime? Period { get; set; }
        public int? ContractId { get; set; }

        public virtual Contract Contract { get; set; }
    }
}
