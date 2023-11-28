using BusinessLayer.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MvcLayer.Models
{
    public class FormViewModel
    {
        public int Id { get; set; }

        [DisplayName("Период составления справки")]
        public DateTime? Period { get; set; }

        [DisplayName("Дата подписания")]
        public DateTime? DateSigning { get; set; }

        [DisplayName("Общая стоимость")]
        public decimal? TotalCost { get; set; }

        [DisplayName("Стоимость СМР")]
        public decimal? SmrCost { get; set; }

        [DisplayName("Стоимость ПНР")]
        public decimal? PnrCost { get; set; }

        [DisplayName("Стоимость оборудования")]
        public decimal? EquipmentCost { get; set; }

        [DisplayName("Стоимость прочих работ")]
        public decimal? OtherExpensesCost { get; set; }

        [DisplayName("Стоимость доп. работ")]
        public decimal? AdditionalCost { get; set; }

        [DisplayName("Материалы заказчика (справочно)")]
        public decimal? MaterialCost { get; set; }

        [DisplayName("Стоимость ген.услуг")]
        public decimal? GenServiceCost { get; set; }

        [DisplayName("Номер")]
        public string Number { get; set; }

        [DisplayName("Собственными силами?")]
        public bool? IsOwnForces { get; set; }
       
        public int? ContractId { get; set; }
        public IFormFileCollection FilesEntity { get; set; }

        [DisplayName("Договор")]
        public virtual ContractDTO Contract { get; set; }
    }
}