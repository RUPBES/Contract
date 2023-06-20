using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Models
{
    public class CorrespondenceDTO
    {

        public int Id { get; set; }

        /// <summary>
        /// Дата письма
        /// </summary>
        public DateTime? Date { get; set; }

        /// <summary>
        /// Номер письма
        /// </summary>
        public string? Number { get; set; }

        /// <summary>
        /// Краткое содержание
        /// </summary>
        public string? Summary { get; set; }

        /// <summary>
        /// Входящее / Исходящее
        /// </summary>
        public bool? IsInBox { get; set; }

        /// <summary>
        /// Контракт
        /// </summary>
        public int? ContractId { get; set; }

        public ContractDTO? Contract { get; set; }

        public List<FileDTO> Files { get; set; } = new List<FileDTO>();
    }
}
