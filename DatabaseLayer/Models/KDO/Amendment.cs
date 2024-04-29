using System;
using System.Collections.Generic;

#nullable disable

namespace DatabaseLayer.Models.KDO
{
    public partial class Amendment
    {
        public int Id { get; set; }
        public string Number { get; set; }
        public DateTime? Date { get; set; }
        public string Reason { get; set; }
        public decimal? ContractPrice { get; set; }
        public DateTime? DateBeginWork { get; set; }
        public DateTime? DateEndWork { get; set; }
        public DateTime? DateEntryObject { get; set; }
        public string ContractChanges { get; set; }
        public string Comment { get; set; }
        public int? ContractId { get; set; }

        public virtual Contract Contract { get; set; }
        public virtual List<AmendmentFile> AmendmentFiles { get; set; }
        public virtual List<MaterialAmendment> MaterialAmendments { get; set; } = new List<MaterialAmendment>();
        public virtual List<PrepaymentAmendment> PrepaymentAmendments { get; set; } = new List<PrepaymentAmendment>();
        public virtual List<ScopeWorkAmendment> ScopeWorkAmendments { get; set; } = new List<ScopeWorkAmendment>();
        public virtual List<ServiceAmendment> ServiceAmendments { get; set; } = new List<ServiceAmendment>();
    }
}
