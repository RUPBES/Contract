using BusinessLayer.Models;

namespace MvcLayer.Models
{
    public class PhoneViewModel
    {
        public int Id { get; set; }

        public string? Number { get; set; }

        public int? OrganizationId { get; set; }

        public int? EmployeeId { get; set; }

        public EmployeeViewModel? Employee { get; set; }

        public OrganizationViewModel? Organization { get; set; }
    }
}
