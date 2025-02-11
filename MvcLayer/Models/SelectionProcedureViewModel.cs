using BusinessLayer.Models;
using BusinessLayer.Models.PRO;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MvcLayer.Models
{
    public class SelectionProcedureViewModel
    {
        
        public int Id { get; set; }

        /// <summary>
        /// Название
        /// </summary>
        [Display(Name ="Наименование закупки")]
        [Required(ErrorMessage = "Обязательное поле")]
        public string? Name { get; set; }

        [Display(Name = "Вид закупки")]
        [Required(ErrorMessage = "Обязательное поле")]
        /// <summary>
        /// Вид закупки
        /// </summary>
        public string? TypeProcedure { get; set; }

        /// <summary>
        /// Срок проведения начало
        /// </summary>
        [Display(Name = "Срок проведения начало")]
        [Required(ErrorMessage = "Обязательное поле")]
        public DateTime? DateBegin { get; set; }

        /// <summary>
        /// Срок проведения окончание
        /// </summary>
        [Display(Name = "Срок проведения окончание")]
        [Required(ErrorMessage = "Обязательное поле")]
        public DateTime? DateEnd { get; set; }

        /// <summary>
        /// Стартовая цена
        /// </summary>
        [Display(Name = "Стартовая цена")]
        [Required(ErrorMessage = "Обязательное поле")]
        public string? StartPrice { get; set; }

        /// <summary>
        /// Цена акцента
        /// </summary>
        [Display(Name = "Цена акцепта")]
        [Required(ErrorMessage = "Обязательное поле")]
        public string? AcceptancePrice { get; set; }

        /// <summary>
        /// Номер акцента
        /// </summary>
        [Display(Name = "Номер акцепта")]
        [Required(ErrorMessage = "Обязательное поле")]
        public string? AcceptanceNumber { get; set; }

        /// <summary>
        /// Дата акцента
        /// </summary>
        [Display(Name = "Дата акцепта")]
        [Required(ErrorMessage = "Обязательное поле")]
        public DateTime? DateAcceptance { get; set; }

        public int? ContractId { get; set; }


        [Required(ErrorMessage = "Обязательное поле")]
        [DisplayName("Файл")]
        public IFormFileCollection FilesEntity { get; set; }

        public virtual ContractViewModel? Contract { get; set; }
        public List<SlctnProcedureFileDTO> ProcedureFiles { get; set; } = new();
    }
}
