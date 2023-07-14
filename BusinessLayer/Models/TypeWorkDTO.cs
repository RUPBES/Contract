using DatabaseLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Models
{
    public class TypeWorkDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public List<TypeWorkContractDTO> TypeWorkContracts { get; set; } =  new List<TypeWorkContractDTO>();
    }
}
