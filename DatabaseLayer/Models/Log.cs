using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseLayer.Models
{
    public class Log
    {
        public int Id { get; set; }
        public string? LogLevel { get; set; }
        public string? Message { get; set; }
        public string? NameSpace { get; set; }
        public string? MethodName { get; set; }
        public string? UserName { get; set; }
        public DateTime? Date { get; set; }
    }
}
