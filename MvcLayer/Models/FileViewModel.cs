using BusinessLayer.Models;

namespace MvcLayer.Models
{
    public class FileViewModel
    {
        public int Id { get; set; }
        public string? FileName { get; set; }
        public string? FilePath { get; set; }
        public string? FileType { get; set; }
        public DateTime? DateUploud { get; set; }

        public List<ActDTO> Acts { get; set; } = new List<ActDTO>();

        public List<AmendmentDTO> Amendments { get; set; } = new List<AmendmentDTO>();

        public List<CorrespondenceDTO> Correspondences { get; set; } = new List<CorrespondenceDTO>();

        public List<EstimateDocDTO> EstimateDocs { get; set; } = new List<EstimateDocDTO>();
        public List<ContractFileDTO> ContractFiles { get; set; } = new List<ContractFileDTO>();
        //public List<СommissionActDTO> СommissionActs { get; set; } = new List<СommissionActDTO>();
    }
}
