using System;
using System.Collections.Generic;

#nullable disable

namespace DatabaseLayer.Models
{
    public partial class TypeWork
    {
        public TypeWork()
        {
            TypeWorkContracts = new HashSet<TypeWorkContract>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<TypeWorkContract> TypeWorkContracts { get; set; }
    }
}
