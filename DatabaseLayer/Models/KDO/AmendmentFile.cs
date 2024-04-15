using System;
using System.Collections.Generic;

#nullable disable

namespace DatabaseLayer.Models.KDO
{
    public partial class AmendmentFile
    {
        public int AmendmentId { get; set; }
        public int FileId { get; set; }

        public virtual Amendment Amendment { get; set; }
        public virtual File File { get; set; }
    }
}
