using BusinessLayer.Models;
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
        public string? Name { get; set; }

        [Display(Name = "Вид закупки")]
        /// <summary>
        /// Вид закупки
        /// </summary>
        public string? TypeProcedure { get; set; }

        /// <summary>
        /// Срок проведения начало
        /// </summary>
        [Display(Name = "Срок проведения начало")]
        public DateTime? DateBegin { get; set; }

        /// <summary>
        /// Срок проведения окончание
        /// </summary>
        [Display(Name = "Срок проведения окончание")]
        public DateTime? DateEnd { get; set; }

        /// <summary>
        /// Стартовая цена
        /// </summary>
        [Display(Name = "Стартовая цена")]
        public decimal? StartPrice { get; set; }

        /// <summary>
        /// Цена акцента
        /// </summary>
        [Display(Name = "Цена акцепта")]
        public decimal? AcceptancePrice { get; set; }

        /// <summary>
        /// Номер акцента
        /// </summary>
        [Display(Name = "Номер акцепта")]
        public string? AcceptanceNumber { get; set; }

        /// <summary>
        /// Дата акцента
        /// </summary>
        [Display(Name = "Дата акцепта")]
        public DateTime? DateAcceptance { get; set; }

        public int? ContractId { get; set; }

        public virtual ContractViewModel? Contract { get; set; }
    }
}
