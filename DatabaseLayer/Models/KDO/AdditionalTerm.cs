using DatabaseLayer.Models.PRO;
using System;
using System.Collections.Generic;

namespace DatabaseLayer.Models.KDO;

public partial class AdditionalTerm
{
    public int Id { get; set; }

    public string? Number { get; set; }

    public DateTime? Date { get; set; }

    public DateTime? DueDate { get; set; }

    public string? Reason { get; set; }

    public bool? IsClaimLitigation { get; set; }

    public string? Type { get; set; }

    public int? ContractId { get; set; }

    public virtual Contract? Contract { get; set; }

    public virtual ICollection<AdditionalTermFile> AdditionalTermFiles { get; set; }
    //public virtual ICollection<File> Files { get; set; } = new List<File>();
}
