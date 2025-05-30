using BusinessLayer.Models.PRO;

namespace MvcLayer.Models;

public class EstimateViewModel
{
    public string BuildingCode { get; set; }
    public List<EstimateItem> Estimates { get; set; } = new();
    public Dictionary<string, EstimateCostResultModel> CostResults { get; set; } = new();
}

public class EstimateItem
{
    public int Id { get; set; }
    public string? Number { get; set; }
    public string BuildingCode { get; set; }
    public string BuildingName { get; set; }
    public DateTime? EstimateDate { get; set; }
    public DateTime? DrawingsDate { get; set; }
    public string DrawingsName { get; set; }
    public DateTime? ChangeDrawingDate { get; set; }
    public string DrawingsKit { get; set; }
    public decimal? ContractsCost { get; set; }
    public double? LaborCost { get; set; }
    public decimal? DoneSmrCost { get; set; }
    public string SubContractor { get; set; }
    public decimal? PercentOfContrPrice { get; set; }
    public decimal? RemainsSmrCost { get; set; }

    public bool IsChange { get; set; }
    public int? ChangeEstimateId { get; set; }
    public DateTime? ChangeEstimateDate { get; set; }
    public int? ChangeNumber { get; set; }
}

public class EstimateCostResultModel
{
    public decimal? ContractsCost { get; set; } = 0M;
    public double? LaborCost { get; set; } = 0;
    public decimal? DoneSmrCost { get; set; } = 0M;
    public decimal? PercentOfContrPrice { get; set; } = 0M;
    public decimal? RemainsSmrCost { get; set; } = 0M;
}