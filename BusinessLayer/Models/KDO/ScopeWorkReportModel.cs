namespace BusinessLayer.Models.KDO;

public class ScopeWorkReportModel
{
    public Dictionary<string, List<ScopeWorkGroup>> Scopes { get; set; } = new();
}

public class ScopeWorkGroup
{
    public string WorkType { get; set; } = string.Empty;
    public decimal Price { get; set; } = 0;
    public decimal? CompletedBeforeYear { get; set; } = 0;
    public decimal? Remaining { get; set; } = 0;
    public decimal? VolumeThisYear { get; set; } = 0;
    public List<Cost>? Costs { get; set; } = new ();
}

public class Cost
{
    public DateTime Period { get; set; }
    public decimal? Value { get; set; }
}
