using BusinessLayer.Models.KDO;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MvcLayer.Models
{
    public class AdditionalTermViewModel
    {
        public int Id { get; set; }

        [DisplayName("Номер")]
        public string? Number { get; set; }

        [DisplayName("Дата")]
        [Required(ErrorMessage = "Заполните дату согласования")]
        public DateTime? Date { get; set; }

        [DisplayName("Дата исполнения обязательств по договору по согласованию")]
        [Required(ErrorMessage = "Заполните дату исполнения обязательств по договору по согласованию")]
        public DateTime? DueDate { get; set; }

        [DisplayName("Причина")]
        [Required(ErrorMessage = "Не указана причина")]
        public string? Reason { get; set; }

        public bool? IsClaimLitigation { get; set; }

        public string? Type { get; set; }

        public int? ContractId { get; set; }

        public virtual ContractDTO? Contract { get; set; }

        [DisplayName("Файл")]
        public IFormFileCollection FilesEntity { get; set; }

        public virtual ICollection<FileDTO> Files { get; set; } = new List<FileDTO>();
    }
}
