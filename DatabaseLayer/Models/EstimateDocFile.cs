using System;
using System.Collections.Generic;

#nullable disable

namespace DatabaseLayer.Models
{
    public partial class EstimateDocFile
    {
        public int EstimateDocId { get; set; }
        public int FileId { get; set; }

        public virtual EstimateDoc EstimateDoc { get; set; }
        public virtual File File { get; set; }
    }
}
