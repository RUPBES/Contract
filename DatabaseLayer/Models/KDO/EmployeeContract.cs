using System;
using System.Collections.Generic;

#nullable disable

namespace DatabaseLayer.Models.KDO
{
    public partial class EmployeeContract
    {
        public int EmployeeId { get; set; }
        public int ContractId { get; set; }
        public bool? IsSignatory { get; set; }
        public bool? IsResponsible { get; set; }

        public virtual Contract Contract { get; set; }
        public virtual Employee Employee { get; set; }
    }
}
