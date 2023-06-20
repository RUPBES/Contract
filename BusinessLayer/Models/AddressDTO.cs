using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Models
{
    public class AddressDTO
    {
        public int Id { get; set; }

        /// <summary>
        /// юр. адрес организации
        /// </summary>
        public string? FullAddress { get; set; }

        /// <summary>
        /// фактический адрес
        /// </summary>
        public string? FullAddressFact { get; set; }

        /// <summary>
        /// Почтовый индекс
        /// </summary>
        public string? PostIndex { get; set; }

        /// <summary>
        /// сайт
        /// </summary>
        public string? SiteAddress { get; set; }

        public int? OrganizationId { get; set; }

        public OrganizationDTO? Organization { get; set; }
    }
}
