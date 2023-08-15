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

        public MaterialGc ChangeMaterial { get; set; }
        public Contract Contract { get; set; }
        public List<MaterialDTO> InverseChangeMaterial { get; set; } = new List<MaterialDTO>();
        public List<MaterialAmendmentDTO> MaterialAmendments { get; set; } = new List<MaterialAmendmentDTO>();
    }
}
