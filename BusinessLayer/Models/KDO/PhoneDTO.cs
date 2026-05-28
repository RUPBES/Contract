using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Models.KDO
{
    public class PhoneDTO
    {
        public int Id { get; set; }


        [RegularExpression(@"^\+?375\d{9}$",
ErrorMessage = "Телефон должен быть в формате +375XXXXXXXXX (12 цифр)")]
        [Display(Name = "Телефон")]
        public string? Number { get; set; }

        public int? OrganizationId { get; set; }

        public int? EmployeeId { get; set; }

        public EmployeeDTO? Employee { get; set; }

        public OrganizationDTO? Organization { get; set; }
    }
}
