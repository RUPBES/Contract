using BusinessLayer.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MvcLayer.Models
{
    public class AddressViewModel
    {
        public int Id { get; set; }

        /// <summary>
        /// юр. адрес организации
        /// </summary>
        [DisplayName("Юр.адрес")]
        public string? FullAddress { get; set; }
        /// <summary>
        /// фактический адрес
        /// </summary>
        /// 
        [DisplayName("Адрес")]
        public string? FullAddressFact { get; set; }

        /// <summary>
        /// сайт
        /// </summary>
        /// 
        [DisplayName("Сайт организации")]
        public string? SiteAddress { get; set; }

        public string? Author { get; set; }

        /// <summary>
        /// Почтовый индекс
        /// </summary>
        [DisplayName("Почтовый индекс")]
        [RegularExpression("[0-9]{6}", ErrorMessage = "Введите 6 цифр")]
        public string? PostIndex { get; set; }

        public int? OrganizationId { get; set; }

        public OrganizationViewModel Organization { get; set; }
    }
}
