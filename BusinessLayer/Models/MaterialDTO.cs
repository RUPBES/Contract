using DatabaseLayer.Models.KDO;
namespace BusinessLayer.Models
{
    public class MaterialDTO
    {
        public int Id { get; set; }        
        public int? ContractId { get; set; }
        public bool? IsChange { get; set; }
        public int? ChangeMaterialId { get; set; }

        public MaterialDTO ChangeMaterial { get; set; }
        public Contract Contract { get; set; }
        public List<MaterialDTO> InverseChangeMaterial { get; set; } = new List<MaterialDTO>();
        public List<MaterialAmendmentDTO> MaterialAmendments { get; set; } = new List<MaterialAmendmentDTO>();
        public List<MaterialCostDTO> MaterialCosts { get; set; } = new List<MaterialCostDTO>();
    }
}
