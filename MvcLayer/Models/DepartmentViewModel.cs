using BusinessLayer.Models;
using System.ComponentModel;

namespace MvcLayer.Models
{
    public class DepartmentViewModel
    {
        public int Id { get; set; }

        [DisplayName("Название")]
        public string? Name { get; set; }
        [DisplayName("Название организации")]
        public int? OrganizationId { get; set; }
        [DisplayName("Название организации")]
        public OrganizationDTO? Organization { get; set; }

        public List<EmployeeDTO> Employees { get; set; } = new List<EmployeeDTO>();
    }
}
