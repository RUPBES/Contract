using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace DatabaseLayer.Models.KDO
{
    public partial class Employee
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Fio { get; set; }
        public string Position { get; set; }
        public string Email { get; set; }
        public string Author { get; set; }

        public virtual List<DepartmentEmployee> DepartmentEmployees { get; set; } = new List<DepartmentEmployee>();
        public virtual List<EmployeeContract> EmployeeContracts { get; set; } = new List<EmployeeContract>();
        public virtual List<Phone> Phones { get; set; } = new List<Phone>();
    }
}
