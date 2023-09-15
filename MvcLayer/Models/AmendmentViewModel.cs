using BusinessLayer.Models;
using System.ComponentModel;

namespace MvcLayer.Models
{
    public class AmendmentViewModel
    {
        
        public int Id { get; set; }

        [DisplayName("Номер")]
        public string Number { get; set; }

        [DisplayName("Дата")]
        public DateTime? Date { get; set; }

        [DisplayName("Причина")]
        public string Reason { get; set; }

        [DisplayName("Договорная (контрактная) цена, руб. с НДС")]
        public decimal? ContractPrice { get; set; }

        [DisplayName("Срок выполнения работ (начало)")]
        public DateTime? DateBeginWork { get; set; }

        [DisplayName("Срок выполнения работ (окончание)")]
        public DateTime? DateEndWork { get; set; }

        [DisplayName("Срок ввода объекта в эксплуатацию")]
        public DateTime? DateEntryObject { get; set; }

        [DisplayName("Существенные изменения пунктов договора")]
        public string ContractChanges { get; set; }

        [DisplayName("Коментарии к изменениям")]
        public string Comment { get; set; }

        [DisplayName("ID Договора")]
        public int? ContractId { get; set; }

        public ContractDTO Contract { get; set; }
        [DisplayName("Файл")]
        public IFormFileCollection FilesEntity { get; set; }

        public List<AmendmentFileDTO> AmendmentFiles { get; set; }
        public List<MaterialAmendmentDTO> MaterialAmendments { get; set; } = new List<MaterialAmendmentDTO>();
        public List<PrepaymentAmendmentDTO> PrepaymentAmendments { get; set; } = new List<PrepaymentAmendmentDTO>();
        public List<ScopeWorkAmendmentDTO> ScopeWorkAmendments { get; set; } = new List<ScopeWorkAmendmentDTO>();
        public List<ServiceAmendmentDTO> ServiceAmendments { get; set; } = new List<ServiceAmendmentDTO>();
    }
}
