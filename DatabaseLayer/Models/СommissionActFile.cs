using System;
using System.Collections.Generic;

#nullable disable

namespace DatabaseLayer.Models
{
    public partial class СommissionActFile
    {
        public int СommissionActId { get; set; }
        public int FileId { get; set; }

        public virtual File File { get; set; }
        public virtual СommissionAct СommissionAct { get; set; }
    }
}
