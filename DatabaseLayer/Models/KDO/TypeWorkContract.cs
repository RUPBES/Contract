using System;
using System.Collections.Generic;

#nullable disable

namespace DatabaseLayer.Models.KDO
{
    public partial class TypeWorkContract
    {
        public int TypeWorkId { get; set; }
        public int ContractId { get; set; }
        public string AdditionalName { get; set; }

        public virtual Contract Contract { get; set; }
        public virtual TypeWork TypeWork { get; set; }
    }
}
