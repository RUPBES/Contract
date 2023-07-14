using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Models
{
    public class IndexViewModel
    {
        public IEnumerable<object> Objects { get; set; }
        public PageViewModel PageViewModel { get; set; }
    }
}
