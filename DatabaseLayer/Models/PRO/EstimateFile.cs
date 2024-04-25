
#nullable disable

namespace DatabaseLayer.Models.PRO
{
    public partial class EstimateFile
    {
        public int EstimateId { get; set; }
        public int FileId { get; set; }

        public virtual Estimate Estimate { get; set; }
        public virtual KDO.File File { get; set; }
    }
}
