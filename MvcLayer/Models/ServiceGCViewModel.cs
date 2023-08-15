using BusinessLayer.Models;
using System.ComponentModel;

namespace MvcLayer.Models
{
    public class ServiceGCViewModel
    {
        public int Id { get; set; }

        [DisplayName("% генуслуг")]
        public int? ServicePercent { get; set; }

        [DisplayName("Период")]
        public DateTime? Period { get; set; }

        [DisplayName("Сумма генуслуг")]
        public decimal? Price { get; set; }

        [DisplayName("Фактическое получение")]
        public decimal? FactPrice { get; set; }

        [DisplayName("ID договора")]
        public int? ContractId { get; set; }

        [DisplayName("Изменено")]
        public bool? IsChange { get; set; }

        [DisplayName("Измененые услуги")]
        public int? ChangeServiceId { get; set; }

        [DisplayName("ID Изменений")]
        public int? AmendmentId { get; set; }

        public ServiceGCDTO ChangeService { get; set; }
        public ContractDTO Contract { get; set; }
        public List<ServiceGCDTO> InverseChangeService { get; set; }
        public List<ServiceAmendmentDTO> ServiceAmendments { get; set; }
    }
}
