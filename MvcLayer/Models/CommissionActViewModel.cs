using BusinessLayer.Models;
using System.ComponentModel;

namespace MvcLayer.Models
{
    public class CommissionActViewModel
    {
        public int Id { get; set; }

        [DisplayName("Номер")]
        public string Number { get; set; }

        [DisplayName("Дата")]
        public DateTime? Date { get; set; }
        public int? ContractId { get; set; }

        [DisplayName("Файл")]
        public IFormFileCollection FilesEntity { get; set; }

        [DisplayName("Договор")]
        public ContractViewModel Contract { get; set; }
        public List<CommissionActFileDTO> СommissionActFiles { get; set; } = new List<CommissionActFileDTO>();
    }
}
