namespace BusinessLayer.Models.Settings
{
    public class EmailOptions
    {
        public static string EmailSettings = "EmailSettings";

        public string From { get; set; }
        public string NameFrom { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string UserLogin { get; set; }
    }
}
