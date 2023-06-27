using System;
using System.Collections.Generic;

#nullable disable

namespace DatabaseLayer.Models
{
    public partial class MaterialGc
    {
        public MaterialGc()
        {
            InverseChangeMaterial = new HashSet<MaterialGc>();
            MaterialAmendments = new HashSet<MaterialAmendment>();
        }

        public int Id { get; set; }
        public decimal? Price { get; set; }
        public decimal? FactPrice { get; set; }
        public DateTime? Period { get; set; }
        public int? ContractId { get; set; }
        public bool? IsChange { get; set; }
        public int? ChangeMaterialId { get; set; }

        public virtual MaterialGc ChangeMaterial { get; set; }
        public virtual Contract Contract { get; set; }
        public virtual ICollection<MaterialGc> InverseChangeMaterial { get; set; }
        public virtual ICollection<MaterialAmendment> MaterialAmendments { get; set; }
    }
}
