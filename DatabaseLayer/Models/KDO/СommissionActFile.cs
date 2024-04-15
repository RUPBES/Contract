using System;
using System.Collections.Generic;

#nullable disable

namespace DatabaseLayer.Models.KDO
{
    public partial class CommissionActFile
    {
        public int СommissionActId { get; set; }
        public int FileId { get; set; }

        public virtual File File { get; set; }
        public virtual CommissionAct СommissionAct { get; set; }
    }
}
