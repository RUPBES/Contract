namespace DatabaseLayer.Models.KDO
{
    public class ContractFile
    {
        public int ContractId { get; set; }
        public int FileId { get; set; }

        public virtual Contract Contract { get; set; }
        public virtual File File { get; set; }
    }
}
