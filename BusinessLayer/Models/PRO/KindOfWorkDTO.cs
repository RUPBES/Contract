using DatabaseLayer.Models.PRO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Models.PRO
{
    public class KindOfWorkDTO
    {
        public int Id { get; set; }
        public string? name { get; set; }
        public virtual List<AbbreviationKindOfWorkDTO> AbbreviationKindOfWorks { get; set; }
    }
}
