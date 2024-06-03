using DatabaseLayer.Models.PRO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Models.PRO
{
    public class EstimateDTO
    {
        public int Id { get; set; }
        public string Number { get; set; }
        public DateTime? EstimateDate { get; set; }
        public string BuildingCode { get; set; }
        public string BuildingName { get; set; }
        public DateTime? DrawingsDate { get; set; }
        public string DrawingsKit { get; set; }        
        public string DrawingsName { get; set; }
        public decimal ContractsCost { get; set; }
        public double LaborCost { get; set; }
        public decimal DoneSmrCost { get; set; }
        public string SubContractor { get; set; }
        public decimal PercentOfContrPrice { get; set; }
        public decimal RemainsSmrCost { get; set; }
        public string Owner { get; set; }
        public int KindOfWorkId { get; set; }

        public virtual AbbreviationKindOfWorkDTO AbbreviationKindOfWorkDTO { get; set; }
        public int ContractId { get; set; }
        public ContractDTO Contract { get; set; }
        public List<EstimateFileDTO> EstimateFiles { get; set; } = new List<EstimateFileDTO>();        
    }
}
