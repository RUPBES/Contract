namespace BusinessLayer.Models
{
    public class DepartmentDTO
    {
        public int Id { get; set; }

        /// <summary>
        /// Название
        /// </summary>
        public string? Name { get; set; }

        public int? OrganizationId { get; set; }

        public OrganizationDTO? Organization { get; set; }

        public List<EmployeeDTO> Employees { get; set; } = new List<EmployeeDTO>();
    }
}
