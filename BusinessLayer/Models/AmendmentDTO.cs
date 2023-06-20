using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Models
{
    public class AmendmentDTO
    {
        public int Id { get; set; }

        /// <summary>
        /// номер изменений
        /// </summary>
        public string? Number { get; set; }

        /// <summary>
        /// дата изменения
        /// </summary>
        public DateTime? Date { get; set; }

        /// <summary>
        /// Причина
        /// </summary>
        public string? Reason { get; set; }

        /// <summary>
        /// договорная (контрактная) цена, руб. с НДС
        /// </summary>
        public decimal? ContractPrice { get; set; }

        /// <summary>
        /// срок выполнения работ (Начало)
        /// </summary>
        public DateTime? DateBeginWork { get; set; }

        /// <summary>
        /// срок выполнения работ (Окончание)
        /// </summary>
        public DateTime? DateEndWork { get; set; }

        /// <summary>
        /// срок ввода объекта в эксплуатацию
        /// </summary>
        public DateTime? DateEntryObject { get; set; }

        /// <summary>
        /// существенные изменения пунктов Договора
        /// </summary>
        public string? ContractChanges { get; set; }

        /// <summary>
        /// коментарии к изменениям
        /// </summary>
        public string? Comment { get; set; }

        /// <summary>
        /// Договор
        /// </summary>
        public int? ContractId { get; set; }

        public ContractDTO? Contract { get; set; }

        public List<FileDTO> Files { get; set; } = new List<FileDTO>();

        //public List<MaterialGcDTO> Materials { get; set; } = new List<MaterialGcDTO>();

        //public List<PrepaymentDTO> Prepayments { get; set; } = new List<PrepaymentDTO>();

        //public List<ScopeWorkDTO> ScopeWorks { get; set; } = new List<ScopeWorkDTO>();

        //public List<ServiceGcDTO> Services { get; set; } = new List<ServiceGcDTO>();
    }
}
