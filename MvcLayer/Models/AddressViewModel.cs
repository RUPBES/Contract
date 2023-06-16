using BusinessLayer.Models;
using System.ComponentModel;

namespace MvcLayer.Models
{
    public class AddressViewModel
    {
        public int Id { get; set; }

        /// <summary>
        /// юр. адрес организации
        /// </summary>
        [DisplayName("Юр.адрес организации")]
        public string? FullAddress { get; set; }

        /// <summary>
        /// Почтовый индекс
        /// </summary>
        [DisplayName("Почтовый индекс")]
        public string? PostIndex { get; set; }

        public int? OrganizationId { get; set; }

        public OrganizationViewModel? Organization { get; set; }
    }
}
