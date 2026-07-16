using BusinessLayer.Models.KDO;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MvcLayer.Models
{
    public class EstimateDocViewModel
    {
        public int Id { get; set; }

        [DisplayName("Номер")]
        [Required(ErrorMessage = "Обязательно для заполнения")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Номер должен содержать от 1 до 50 символов")]
        public string Number { get; set; }

        [DisplayName("Дата изменения ПСД")]
        [Required(ErrorMessage = "Необходимо выбрать дату")]
        public DateTime? DateChange { get; set; }

        [DisplayName("Дата выхода смет")]
        [Required(ErrorMessage = "Необходимо выбрать дату")]
        public DateTime? DateOutput { get; set; }

        [DisplayName("Причины изменения ПСД")]
        [Required(ErrorMessage = "Обязательно для заполнения")]
        [StringLength(500, MinimumLength = 7, ErrorMessage = "Должно содержать от 7 до 500 символов")]
        public string Reason { get; set; }
        public int? ContractId { get; set; }
        public bool? IsChange { get; set; }

        [DisplayName("Файл")]
        [Required(ErrorMessage = "Необходимо прикрепить файл")]
        public IFormFileCollection FilesEntity { get; set; }

        [DisplayName("Договор")]
        public ContractViewModel Contract { get; set; }
        public List<EstimateDocFileDTO> EstimateDocFiles { get; set; } = new List<EstimateDocFileDTO>();
    }
}
