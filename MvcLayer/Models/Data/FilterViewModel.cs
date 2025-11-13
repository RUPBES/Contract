namespace MvcLayer.Models.Data
{
    public class FilterViewModel
    {
        public string? GenContractor { get; set; }
        public string? Client { get; set; }
        public DateTime? DateEnteringTerm { get; set; }
        public DateTime? StarEnteringTerm { get; set; }
        public DateTime? EndEnteringTerm { get; set; }
    }
}