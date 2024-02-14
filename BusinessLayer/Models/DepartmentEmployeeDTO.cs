using DatabaseLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Models
{
    public class DepartmentEmployeeDTO
    {
        public int DepartmentId { get; set; }
        public int EmployeeId { get; set; }

        public virtual DepartmentDTO Department { get; set; }
        public virtual EmployeeDTO Employee { get; set; }
    }
}
