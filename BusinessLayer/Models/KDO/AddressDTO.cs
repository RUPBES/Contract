using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Models.KDO
{
    public class AddressDTO
    {
        public int Id { get; set; }

        /// <summary>
        /// юр. адрес организации
        /// </summary>
        /// 
        [Required(ErrorMessage = "Обязательно для заполнения")]
        [StringLength(500, MinimumLength = 7, ErrorMessage = "Название должно содержать от 7 до 500 символов")]
        [Display(Name = "Юр.Адрес")]
        public string? FullAddress { get; set; }

        /// <summary>
        /// фактический адрес
        /// </summary>
        [StringLength(500, MinimumLength = 7, ErrorMessage = "Название должно содержать от 7 до 500 символов")]
        [Display(Name = "Адрес")]
        public string? FullAddressFact { get; set; }

        /// <summary>
        /// Почтовый индекс
        /// </summary>
        /// 
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Почтовый индекс должен состоять из 6 цифр")]
        [Display(Name = "Почтовый индекс")]
        public string? PostIndex { get; set; }

        /// <summary>
        /// сайт
        /// </summary>
        /// 
        [Url(ErrorMessage = "Введите корректный адрес сайта (например: https://example.by)")]
        [Display(Name = "Сайт")]
        public string? SiteAddress { get; set; }

        public int? OrganizationId { get; set; }

        public OrganizationDTO? Organization { get; set; }
    }
}
