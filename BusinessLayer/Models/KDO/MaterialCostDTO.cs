using System.ComponentModel;

namespace BusinessLayer.Models.KDO
{
    public class MaterialCostDTO
    {
        public int Id { get; set; }
        public DateTime? Period { get; set; }
        [DisplayName("Стоимость")]
        public decimal? Price { get; set; }
        public bool? IsFact { get; set; }
        public int? MaterialId { get; set; }
        public virtual MaterialDTO? Material { get; set; }
    }
}