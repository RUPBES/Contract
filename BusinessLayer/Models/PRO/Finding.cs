namespace BusinessLayer.Models.PRO
{
    public class Finding
    {
        public SearchEstimateObject Estimate { get; set; }
        public SearchObject LaborCost { get; set; }
        public SearchObject ContractCost { get; set; }
        public SearchObject DoneSmrCost { get; set; }
    }
    public class SearchEstimateObject
    {
        public List<string> DocName { get; set; }
        public List<string> BuildingName { get; set; }
        public List<string> BuildingCode { get; set; }

        public List<string> DrawingKit { get; set; }
        public List<string> StartLineLookingForEstimateName { get; set; }
    }
    public class SearchObject
    {
        public List<string> DocName { get; set; }
        public List<string> ColName { get; set; }
        public List<string> RowName { get; set; }
        public List<string> ExtraColName { get; set; }
        public List<string> ExtraRowName { get; set; }
    }
}
