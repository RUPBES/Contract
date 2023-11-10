using DatabaseLayer.Models;

namespace BusinessLayer.Models
{
    public class FileDTO
    {
        public int Id { get; set; }

        /// <summary>
        /// Имя файла
        /// </summary>
        public string? FileName { get; set; }

        /// <summary>
        /// Путь к файлу
        /// </summary>
        public string? FilePath { get; set; }

        /// <summary>
        /// Тип документа
        /// </summary>
        public string? FileType { get; set; }

        /// <summary>
        /// Дата загрузки
        /// </summary>
        public DateTime? DateUploud { get; set; }

        public List<ActDTO> Acts { get; set; } = new List<ActDTO>();

        public List<AmendmentDTO> Amendments { get; set; } = new List<AmendmentDTO>();

        public List<CorrespondenceDTO> Correspondences { get; set; } = new List<CorrespondenceDTO>();

        public List<EstimateDocDTO> EstimateDocs { get; set; } = new List<EstimateDocDTO>();
        public List<ContractFileDTO> ContractFiles { get; set; } = new List<ContractFileDTO>();

        //public List<СommissionActDTO> СommissionActs { get; set; } = new List<СommissionActDTO>();
    }
}
