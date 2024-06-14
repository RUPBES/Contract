using System;
using System.Collections.Generic;

#nullable disable

namespace DatabaseLayer.Models.PRO
{
    public partial class Estimate
    {
        public Estimate()
        {
            EstimateFiles = new HashSet<EstimateFile>();
        }

        public int Id { get; set; }
        public string? Number { get; set; }
        public DateTime? EstimateDate { get; set; }
        public string BuildingCode { get; set; }
        public string BuildingName { get; set; }
        public DateTime? DrawingsDate { get; set; }       
        public string DrawingsKit { get; set; }        
        public string DrawingsName { get; set; }
        public decimal? ContractsCost { get; set; }
        public double? LaborCost { get; set; }
        public decimal? DoneSmrCost { get; set; }
        public string SubContractor { get; set; }
        public decimal? PercentOfContrPrice { get; set; }
        public decimal? RemainsSmrCost { get; set; }
        public string Owner { get; set; }
        public int KindOfWorkId { get; set; }

        public virtual AbbreviationKindOfWork AbbreviationKindOfWork { get; set; }
        public int ContractId { get; set; }
        public virtual KDO.Contract Contract { get; set; }
        public virtual ICollection<EstimateFile> EstimateFiles { get; set; }
    }
}
