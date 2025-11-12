namespace BusinessLayer.Models.Settings
{
    public class SchedulerOptions
    {
        public const string Scheduler = "Scheduler";

        public int DaysCount { get; set; } = 7;
        public string BaseUrl { get; set; }
    }
}


