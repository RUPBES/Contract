using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Models
{
    public class UserActivity
    {
        public string? NameSpace { get; set; }
        public string? MethodName { get; set; }
        public string? UserName { get; set; }
        public string? DateTime { get; set; }
    }
}
