using System;
using System.Collections.Generic;

#nullable disable

namespace DatabaseLayer.Models
{
    public partial class ScopeWorkAmendment
    {
        public int ScopeWorkId { get; set; }
        public int AmendmentId { get; set; }

        public virtual Amendment Amendment { get; set; }
        public virtual ScopeWork ScopeWork { get; set; }
    }
}
