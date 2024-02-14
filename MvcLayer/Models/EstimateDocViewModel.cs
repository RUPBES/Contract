using BusinessLayer.Models;
using System.ComponentModel;

namespace MvcLayer.Models
{
    public class EstimateDocViewModel
    {
        public int Id { get; set; }

        [DisplayName("Номер")]
        public string Number { get; set; }

        [DisplayName("Дата изменения ПСД")]
        public DateTime? DateChange { get; set; }

        [DisplayName("Дата выхода смет")]
        public DateTime? DateOutput { get; set; }

        [DisplayName("Причины изменения ПСД")]
        public string Reason { get; set; }
        public int? ContractId { get; set; }
        public bool? IsChange { get; set; }
        [DisplayName("Файл")]
        public IFormFileCollection FilesEntity { get; set; }

        [DisplayName("Договор")]
        public ContractViewModel Contract { get; set; }
        public List<EstimateDocFileDTO> EstimateDocFiles { get; set; } = new List<EstimateDocFileDTO>();
    }
}
