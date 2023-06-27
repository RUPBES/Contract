using System;
using System.Collections.Generic;

#nullable disable

namespace DatabaseLayer.Models
{
    public partial class ActFile
    {
        public int ActId { get; set; }
        public int FileId { get; set; }

        public virtual Act Act { get; set; }
        public virtual Models.File File { get; set; }
    }
}
