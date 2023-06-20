using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Models
{
    public class PhoneDTO
    {
        public int Id { get; set; }

        public string? Number { get; set; }

        public int? OrganizationId { get; set; }

        public int? EmployeeId { get; set; }

        public EmployeeDTO? Employee { get; set; }

        public OrganizationDTO? Organization { get; set; }
    }
}
