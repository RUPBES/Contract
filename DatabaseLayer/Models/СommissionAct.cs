using System;
using System.Collections.Generic;

#nullable disable

namespace DatabaseLayer.Models
{
    public partial class CommissionAct
    {
        public CommissionAct()
        {
            СommissionActFiles = new HashSet<CommissionActFile>();
        }

        public int Id { get; set; }
        public string Number { get; set; }
        public DateTime? Date { get; set; }
        public int? ContractId { get; set; }

        public virtual Contract Contract { get; set; }
        public virtual ICollection<CommissionActFile> СommissionActFiles { get; set; }
    }
}
