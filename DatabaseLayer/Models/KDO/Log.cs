using System;
using System.Collections.Generic;

#nullable disable

namespace DatabaseLayer.Models.KDO
{
    public partial class Log
    {
        public int Id { get; set; }
        public string LogLevel { get; set; }
        public string Message { get; set; }
        public string NameSpace { get; set; }
        public string MethodName { get; set; }
        public string UserName { get; set; }
        public DateTime? DateTime { get; set; }
    }
}
