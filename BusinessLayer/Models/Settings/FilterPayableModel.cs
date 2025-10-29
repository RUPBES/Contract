namespace BusinessLayer.Models.Settings
{
    public class FilterPayableModel
    {
        public string? GenContractor { get; set; } = null;

        public string? Client { get; set; } = null;

        public DateTime? DateEnteringTerm { get; set; } = null; //срок ввода

        public DateTime? StarEnteringTerm { get; set; } = null; //срок ввода начало
        public DateTime? EndEnteringTerm { get; set; } = null; //срок ввода конец

        public DateTime? DateBeginWork { get; set; }

        public DateTime? DateEndWork { get; set; }
    }
}
