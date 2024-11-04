namespace BusinessLayer.Models.Settings
{
    public class ExcelActivityReportOptions
    {
        public const string ExcelActivityReport = "ExcelActivityReport";
        public string Path { get; set; }
        public string Directory { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public string SheetName { get; set; }

    }
}
