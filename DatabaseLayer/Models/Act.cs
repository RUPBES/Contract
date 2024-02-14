using System;
using System.Collections.Generic;

#nullable disable

namespace DatabaseLayer.Models
{
    public partial class Act
    {
        public int Id { get; set; }
        public string Reason { get; set; }
        public DateTime? DateAct { get; set; }
        public DateTime? DateSuspendedFrom { get; set; }
        public DateTime? DateSuspendedUntil { get; set; }
        public DateTime? DateRenewal { get; set; }
        public bool? IsSuspension { get; set; }
        public int? ContractId { get; set; }

        public virtual Contract Contract { get; set; }
        public virtual List<ActFile> ActFiles { get; set; } = new List<ActFile>();
    }
}
