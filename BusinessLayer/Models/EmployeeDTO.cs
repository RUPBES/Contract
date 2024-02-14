using DatabaseLayer.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Models
{
    public class EmployeeDTO
    {
        public int Id { get; set; }

        public string? FullName { get; set; }

        public string? FirstName { get; set; }
        
        public string? LastName { get; set; }
       
        public string? FatherName { get; set; }

        public string? Fio { get; set; }

        public string? Position { get; set; }

        public string? Email { get; set; }
        public string? Author { get; set; }

        public List<EmployeeContractDTO> EmployeeContracts { get; set; } = new List<EmployeeContractDTO>();

        public List<PhoneDTO> Phones { get; set; } = new List<PhoneDTO>();
        public virtual List<DepartmentEmployeeDTO> DepartmentEmployees { get; set; } = new List<DepartmentEmployeeDTO>();
    }
}
