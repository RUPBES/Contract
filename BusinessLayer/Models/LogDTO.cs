using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Models
{
    public class LogDTO
    {
        public string? LogLevel { get; set; }
        public string? Message { get; set; }
        public string? NameSpace { get; set; }
        public string? MethodName { get; set; }
        public string? UserName { get; set; }
        public string? UserIdentifierOid { get; set; }
        public DateTime? Date { get; set; }
    }
}
