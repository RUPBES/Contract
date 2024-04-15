using DatabaseLayer.Models.KDO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Models
{
    public class EstimateDocDTO
    {
        public int Id { get; set; }
        public string Number { get; set; }
        public DateTime? DateChange { get; set; }
        public DateTime? DateOutput { get; set; }
        public string Reason { get; set; }
        public int? ContractId { get; set; }
        public bool? IsChange { get; set; }

        public Contract Contract { get; set; }
        public List<EstimateDocFileDTO> EstimateDocFiles { get; set; } = new List<EstimateDocFileDTO>();
    }
}
