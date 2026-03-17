using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseLayer.Models.EXTRA
{
    public class EmployeeRecord
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

        public string? PhoneNumbers { get; set; }
    }
}
