using DatabaseLayer.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Models.KDO
{
    public class OrganizationDTO
    {
        public int Id { get; set; }

        /// <summary>
        /// Полное название
        /// </summary>
        [Required(ErrorMessage = "Обязательно для заполнения")]
        [StringLength(500, MinimumLength = 2, ErrorMessage = "Название должно содержать от 2 до 500 символов")]
        [Display(Name = "Полное название")]
        public string? Name { get; set; }

        /// <summary>
        /// Аббревиатура
        /// </summary>
        /// 
        [Required(ErrorMessage = "Обязательно для заполнения")]
        [StringLength(500, MinimumLength = 2, ErrorMessage = "Название должно содержать от 2 до 500 символов")]
        [Display(Name = "Аббревиатура")]
        public string? Abbr { get; set; }

        /// <summary>
        /// УНП предприятия
        /// </summary>
        /// 
        [Required(ErrorMessage = "Обязательно для заполнения")]
        [RegularExpression(@"^\d{9}$", ErrorMessage = "УНП должен состоять из 9 цифр")]
        [StringLength(9, MinimumLength = 9, ErrorMessage = "УНП должен содержать ровно 9 символов")]
        [Display(Name = "УНП")]
        public string? Unp { get; set; }

        /// <summary>
        /// электронная почта
        /// </summary>
        /// 
        [EmailAddress(ErrorMessage = "Введите корректный адрес электронной почты")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        /// <summary>
        /// расчетный счет
        /// </summary>
        /// 
        [RegularExpression(@"^[BY]{2}\d{2}[A-Z0-9]{4}\d{16}$",    ErrorMessage = "Расчётный счёт должен быть в формате IBAN (28 символов)")]
        [Display(Name = "Расчётный счёт")]
        public string? PaymentAccount { get; set; }

        [Required(ErrorMessage = "Обязательно для заполнения")]
        [StringLength(500, MinimumLength = 7, ErrorMessage = "Название должно содержать от 7 до 500 символов")]
        [Display(Name = "Юр.Адрес")]
        public string FullAddress { get; set; }
        public string FullAddressFact { get; set; }

        [RegularExpression(@"^\d{6}$", ErrorMessage = "Почтовый индекс должен состоять из 6 цифр")]
        [Display(Name = "Почтовый индекс")]
        public string PostIndex { get; set; }

        [Url(ErrorMessage = "Введите корректный адрес сайта (например: https://example.by)")]
        [Display(Name = "Сайт")]
        public string SiteAddress { get; set; }

        [RegularExpression(@"^\+?375\d{9}$",
    ErrorMessage = "Телефон должен быть в формате +375XXXXXXXXX (12 цифр)")]
        [Display(Name = "Телефон")]
        public string PhoneNumbers { get; set; }

        public List<AddressDTO> Addresses { get; set; } = new List<AddressDTO>();

        public List<ContractOrganizationDTO> ContractOrganizations { get; set; } = new List<ContractOrganizationDTO>();

        public List<DepartmentDTO> Departments { get; set; } = new List<DepartmentDTO>();

        public List<PhoneDTO> Phones { get; set; } = new List<PhoneDTO>();
    }
}
