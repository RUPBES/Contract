using System.ComponentModel.DataAnnotations;

namespace BusinessLayer.Models.KDO
{
    public class TypeWorkDTO
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Название типа работ обязательно")]
        [StringLength(150, MinimumLength = 5,
    ErrorMessage = "Название типа работ должно быть от 5 до 150 символов")]
        [Display(Name = "Название типа работ")]
        public string Name { get; set; }

        public List<TypeWorkContractDTO> TypeWorkContracts { get; set; } = new List<TypeWorkContractDTO>();
    }
}
