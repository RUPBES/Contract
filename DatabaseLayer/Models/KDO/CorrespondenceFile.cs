using System;
using System.Collections.Generic;

#nullable disable

namespace DatabaseLayer.Models.KDO
{
    public partial class CorrespondenceFile
    {
        public int CorrespondenceId { get; set; }
        public int FileId { get; set; }

        public virtual Correspondence Correspondence { get; set; }
        public virtual File File { get; set; }
    }
}
