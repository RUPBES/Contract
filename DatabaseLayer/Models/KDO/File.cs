#nullable disable

using DatabaseLayer;
using DatabaseLayer.Models.PRO;

namespace DatabaseLayer.Models.KDO
{
    public partial class File
    {
        public File()
        {
            ActFiles = new HashSet<ActFile>();
            AmendmentFiles = new HashSet<AmendmentFile>();
            CorrespondenceFiles = new HashSet<CorrespondenceFile>();
            EstimateDocFiles = new HashSet<EstimateDocFile>();
            СommissionActFiles = new HashSet<CommissionActFile>();
            ContractFiles = new HashSet<ContractFile>();
            EstimateFiles = new HashSet<EstimateFile>();
        }

        public int Id { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string FileType { get; set; }
        public DateTime? DateUploud { get; set; }

        public virtual ICollection<EstimateFile> EstimateFiles { get; set; }

        public virtual ICollection<ActFile> ActFiles { get; set; }
        public virtual ICollection<FormFile> FormFiles { get; set; }
        public virtual ICollection<AmendmentFile> AmendmentFiles { get; set; }
        public virtual ICollection<CorrespondenceFile> CorrespondenceFiles { get; set; }
        public virtual ICollection<EstimateDocFile> EstimateDocFiles { get; set; }
        public virtual ICollection<ContractFile> ContractFiles { get; set; }
        public virtual ICollection<CommissionActFile> СommissionActFiles { get; set; }
        public virtual ICollection<PrepaymentTake> PrepaymentTakesFiles { get; set; }
    }
}
