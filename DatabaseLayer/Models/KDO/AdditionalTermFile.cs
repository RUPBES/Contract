namespace DatabaseLayer.Models.KDO
{
    public class AdditionalTermFile
    {
        public int AdditionalTermId { get; set; }
        public int FileId { get; set; }

        public virtual AdditionalTerm AdditionalTerm { get; set; }
        public virtual DatabaseLayer.Models.KDO.File File { get; set; }
    }
}
