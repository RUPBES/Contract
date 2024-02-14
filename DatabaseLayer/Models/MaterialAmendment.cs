using System;
using System.Collections.Generic;

#nullable disable

namespace DatabaseLayer.Models
{
    public partial class MaterialAmendment
    {
        public int MaterialId { get; set; }
        public int AmendmentId { get; set; }

        public virtual Amendment Amendment { get; set; }
        public virtual MaterialGc Material { get; set; }
    }
}
