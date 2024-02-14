using System.ComponentModel;

namespace MvcLayer.Models.Reports
{
    public class GetCostDeviationMaterialViewModel
    {
        [DisplayName("Идентификатор")]
        public int Id { get; set; }
        [DisplayName("Дата договора")]
        public DateTime? dateContract { get; set; }
        [DisplayName("Дата начала работ")]
        public DateTime? dateBeginWork { get; set; }
        [DisplayName("Дата конца работ")]
        public DateTime? dateEndWork { get; set; }
        [DisplayName("Номер договора")]
        public string? number { get; set; }
        [DisplayName("Название объекта")]
        public string? nameObject { get; set; }
        [DisplayName("Предпологаемая стоимость материалов по контракту")]
        public decimal? materialCost { get; set; }
        [DisplayName("Материалы")]
        public List<ItemMaterialDeviationReport>? listMaterials { get; set; }
        [DisplayName("Тип контракта")]
        public string? typeContract { get; set; }
    }

    public class ItemMaterialDeviationReport
    {
        [DisplayName("Период")]
        public DateTime? period { get; set; }
        [DisplayName("Планируемое значение")]
        public decimal? plan { get; set; }
        [DisplayName("Фактическое значение")]
        public decimal? fact { get; set; }
    }
}
