using System;
using System.Collections.Generic;

#nullable disable

namespace DatabaseLayer.Models.KDO
{
    public partial class EstimateDoc
    {
        public EstimateDoc()
        {
            EstimateDocFiles = new HashSet<EstimateDocFile>();
        }

        public int Id { get; set; }
        public string Number { get; set; }
        public DateTime? DateChange { get; set; }
        public DateTime? DateOutput { get; set; }
        public string Reason { get; set; }
        public int? ContractId { get; set; }
        public bool? IsChange { get; set; }

        public virtual Contract Contract { get; set; }
        public virtual ICollection<EstimateDocFile> EstimateDocFiles { get; set; }
    }
}
