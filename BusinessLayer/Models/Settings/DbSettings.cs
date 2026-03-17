
namespace BusinessLayer.Models.Settings
{
    public class DbSettings
    {
        public static string ConnectionStrings = "ConnectionStrings";

        public string SourceArchiveDb { get; set; }
        public string TargetArchiveDb { get; set; }
    }
}
