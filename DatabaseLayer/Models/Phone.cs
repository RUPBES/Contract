using System;
using System.Collections.Generic;

#nullable disable

namespace DatabaseLayer.Models
{
    public partial class Phone
    {
        public int Id { get; set; }
        public string Number { get; set; }
        public int? OrganizationId { get; set; }
        public int? EmployeeId { get; set; }

        public virtual Employee Employee { get; set; }
        public virtual Organization Organization { get; set; }
    }
}
