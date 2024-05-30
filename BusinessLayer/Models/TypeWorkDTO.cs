namespace BusinessLayer.Models
{
    public class TypeWorkDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public List<TypeWorkContractDTO> TypeWorkContracts { get; set; } =  new List<TypeWorkContractDTO>();
    }
}
