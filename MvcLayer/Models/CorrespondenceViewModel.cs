using BusinessLayer.Models.KDO;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MvcLayer.Models
{
    public class CorrespondenceViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Необходимо выбрать дату")]
        [DisplayName("Дата письма")]
        public DateTime? Date { get; set; }

        [Required(ErrorMessage = "Обязательно для заполнения")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Номер должен содержать от 1 до 50 символов")]
        [DisplayName("Номер письма")]
        public string? Number { get; set; }

        [Required(ErrorMessage = "Обязательно для заполнения")]
        [StringLength(500, MinimumLength = 7, ErrorMessage = "Должно содержать от 7 до 500 символов")]
        [DisplayName("Краткое содержание")]
        public string? Summary { get; set; }

        [DisplayName("Тип письма")]
        public bool IsInBox { get; set; }

        public int? ContractId { get; set; }

        [DisplayName("Файл")]
        [Required(ErrorMessage = "Необходимо прикрепить файл")]
        public IFormFileCollection FilesEntity { get; set; }

        [DisplayName("Договор")]
        public ContractDTO? Contract { get; set; }

       
        public List<FileDTO> Files { get; set; } = new List<FileDTO>();
    }
}
