using System;
using System.Collections.Generic;

#nullable disable

namespace DatabaseLayer.Models
{
    public partial class SelectionProcedure
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string TypeProcedure { get; set; }
        public DateTime? DateBegin { get; set; }
        public DateTime? DateEnd { get; set; }
        public decimal? StartPrice { get; set; }
        public decimal? AcceptancePrice { get; set; }
        public string AcceptanceNumber { get; set; }
        public DateTime? DateAcceptance { get; set; }
        public int? ContractId { get; set; }

        public virtual Contract Contract { get; set; }
    }
}
