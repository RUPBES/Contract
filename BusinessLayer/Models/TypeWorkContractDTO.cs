using DatabaseLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Models
{
    public class TypeWorkContractDTO
    {
        public int TypeWorkId { get; set; }
        public int ContractId { get; set; }
        public string AdditionalName { get; set; }

        public virtual ContractDTO Contract { get; set; }
        public virtual TypeWorkDTO TypeWork { get; set; }
    }
}
