using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Models.Settings
{
    public class ArchiveSettings
    {
        public static string ConnectionStrings = "ConnectionStrings";

        public string SourceArchiveDb { get; set; }
        public string TargetArchiveDb { get; set; }
    }
}
