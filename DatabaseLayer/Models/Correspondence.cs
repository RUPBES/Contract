using System;
using System.Collections.Generic;

#nullable disable

namespace DatabaseLayer.Models
{
    public partial class Correspondence
    {
        public Correspondence()
        {
            CorrespondenceFiles = new HashSet<CorrespondenceFile>();
        }

        public int Id { get; set; }
        public DateTime? Date { get; set; }
        public string Number { get; set; }
        public string Summary { get; set; }
        public bool? IsInBox { get; set; }
        public int? ContractId { get; set; }

        public virtual Contract Contract { get; set; }
        public virtual ICollection<CorrespondenceFile> CorrespondenceFiles { get; set; }
    }
}
