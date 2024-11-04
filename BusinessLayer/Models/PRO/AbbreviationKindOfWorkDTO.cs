using DatabaseLayer.Models.PRO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Models.PRO
{
    public class AbbreviationKindOfWorkDTO
    {
        public int Id { get; set; }
        public int KindOfWorkId { get; set; }
        public string? Name { get; set; }

        public virtual KindOfWorkDTO KindOfWork { get; set; }
        public virtual List<AbbreviationKindOfWorkDTO> Abbreviations { get; set; }
    }
}
