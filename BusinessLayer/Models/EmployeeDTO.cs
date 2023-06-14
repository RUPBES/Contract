using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Models
{
    public class EmployeeDTO
    {
        public int Id { get; set; }

        public string? FullName { get; set; }

        public string? Fio { get; set; }

        public string? Position { get; set; }

        public string? Email { get; set; }

        public int? ContractId { get; set; }

        public ContractDTO? Contract { get; set; }

        public List<PhoneDTO> Phones { get; set; } = new List<PhoneDTO>();

        public List<DepartmentDTO> Departments { get; set; } = new List<DepartmentDTO>();
    }
}
