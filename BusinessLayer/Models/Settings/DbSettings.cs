
namespace BusinessLayer.Models.Settings
{
    public class DbSettings
    {
        public static string ConnectionStrings = "ConnectionStrings";

        /// <summary>
        /// Текущая БД
        /// </summary>
        public string SourceArchiveDb { get; set; }
        /// <summary>
        /// Архивная БД
        /// </summary>
        public string TargetArchiveDb { get; set; }
    }
}
