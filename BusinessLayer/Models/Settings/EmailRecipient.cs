namespace BusinessLayer.Models.Settings
{
    public class EmailRecipient
    {
        public static string EmailRecipients = "EmailRecipients";

        public IEnumerable<string> ReportActivities { get; set; }
    }
}
