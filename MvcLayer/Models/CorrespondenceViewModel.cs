using BusinessLayer.Models;
using System.ComponentModel;

namespace MvcLayer.Models
{
    public class CorrespondenceViewModel
    {
        public int Id { get; set; }

        [DisplayName("Дата письма")]
        public DateTime? Date { get; set; }

        [DisplayName("Номер письма")]
        public string? Number { get; set; }

        [DisplayName("Краткое содержание")]
        public string? Summary { get; set; }

        [DisplayName("Тип письма")]
        public bool IsInBox { get; set; }

        public int? ContractId { get; set; }
        [DisplayName("Файл")]
        public IFormFileCollection FilesEntity { get; set; }

        [DisplayName("Договор")]
        public ContractDTO? Contract { get; set; }

        public List<FileDTO> Files { get; set; } = new List<FileDTO>();
    }
}
