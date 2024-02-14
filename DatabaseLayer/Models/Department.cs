using System;
using System.Collections.Generic;

#nullable disable

namespace DatabaseLayer.Models
{
    public partial class Department
    {

        public int Id { get; set; }
        public string Name { get; set; }
        public int? OrganizationId { get; set; }

        public virtual Organization Organization { get; set; }
        public virtual List<DepartmentEmployee> DepartmentEmployees { get; set; } = new List<DepartmentEmployee>();
    }
}
