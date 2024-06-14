using BusinessLayer.Models.PRO;
using BusinessLayer.Models;

namespace MvcLayer.Models
{
    public class EstimateViewModel
    {
        public string BuildingCode { get; set; }
        public string BuildingName { get; set; }
        public Dictionary<string, EstimateViewResultBuilding> report { get; set; } = new Dictionary<string, EstimateViewResultBuilding>();
        public List<EstimateViewModelItem> DetailsView { get; set; } = new List<EstimateViewModelItem>();
    }

    public class EstimateViewModelItem
    {
        public string DrawingsName { get; set; }
        public int EstimateCount { get; set; } = 0;
        public List<int> NumberEntriesByEstimate { get; set; } = new List<int>();
        public List<EstimateViewModelDrawning> EstimateViewModelDrawnings { get; set; } = new List<EstimateViewModelDrawning>();
    }

    public class EstimateViewModelDrawning
    {
        public int Id { get; set; }
        public string? Number { get; set; }
        public DateTime? EstimateDate { get; set; }
        public DateTime? EstimateChangeDate { get; set; }
        public DateTime? DrawingsDate { get; set; }
        public DateTime? DrawingsChangeDate { get; set; }
        public string DrawingsKit { get; set; }
        public decimal? ContractsCost { get; set; }
        public double? LaborCost { get; set; }
        public decimal? DoneSmrCost { get; set; }
        public string SubContractor { get; set; }
        public decimal? PercentOfContrPrice { get; set; }
        public decimal? RemainsSmrCost { get; set; }
        public string Owner { get; set; }
        public string KindOfWork { get; set; }
        public List<EstimateFileDTO> EstimateFiles { get; set; } = new List<EstimateFileDTO>();
    }

    public class EstimateViewResultBuilding
    {
        public EstimateViewResultBuilding()
        {
            ContractsCost = 0;
            LaborCost = 0;
            DoneSmrCost = 0;
            PercentOfContrPrice = 0;
            RemainsSmrCost = 0;
        }

        public decimal? ContractsCost { get; set; }
        public double? LaborCost { get; set; }
        public decimal? DoneSmrCost { get; set; }
        public decimal? PercentOfContrPrice { get; set; }
        public decimal? RemainsSmrCost { get; set; }
    }
}
