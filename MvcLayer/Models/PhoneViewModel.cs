using System.ComponentModel.DataAnnotations;

namespace MvcLayer.Models
{
    public class PhoneViewModel
    {

        public int Id { get; set; }


        [RegularExpression(@"^\+?375\d{9}$", ErrorMessage = "Телефон должен быть в формате +375XXXXXXXXX (12 цифр)")]
        [Display(Name = "Телефон")]
        public string? Number { get; set; }

        public int? OrganizationId { get; set; }

        public int? EmployeeId { get; set; }

        public EmployeeViewModel? Employee { get; set; }

        public OrganizationViewModel? Organization { get; set; }
    }
}
