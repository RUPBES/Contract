using BusinessLayer.Models;
using DatabaseLayer.Models;
using System.ComponentModel;

namespace MvcLayer.Models
{
    public class MaterialViewModel
    {
        public int Id { get; set; }

        [DisplayName("Сметная стоимость по плану")]
        public decimal? Price { get; set; }

        [DisplayName("Сметная стоимость факт")]
        public decimal? FactPrice { get; set; }

        [DisplayName("Период")]
        public DateTime? Period { get; set; }
        public int? ContractId { get; set; }

        [DisplayName("Изиенено")]
        public bool? IsChange { get; set; }
        public int? ChangeMaterialId { get; set; }

        [DisplayName("ID Изменений")]
        public int? AmendmentId { get; set; }

        [DisplayName("По факту?")]
        public bool? IsFact { get; set; }
       
        public ContractViewModel Contract { get; set; }
        public List<MaterialViewModel> InverseChangeMaterial { get; set; } = new List<MaterialViewModel>();
        public List<MaterialAmendmentDTO> MaterialAmendments { get; set; } = new List<MaterialAmendmentDTO>();
        public List<MaterialCostDTO> MaterialCosts { get; set; } = new List<MaterialCostDTO>();
    }
}
