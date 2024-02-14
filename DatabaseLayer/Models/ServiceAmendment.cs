using System;
using System.Collections.Generic;

#nullable disable

namespace DatabaseLayer.Models
{
    public partial class ServiceAmendment
    {
        public int ServiceId { get; set; }
        public int AmendmentId { get; set; }

        public virtual Amendment Amendment { get; set; }
        public virtual ServiceGc Service { get; set; }
    }
}
