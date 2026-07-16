using System;
using System.Collections.Generic;

namespace DatabaseLayer.Models.KDO;

public partial class VContract
{
    public int Id { get; set; }

    public string? Number { get; set; }
    public int? SubContractId { get; set; }
    public int? AgreementContractId { get; set; }
    public DateTime? Date { get; set; }
    public DateTime? EnteringTerm { get; set; }
    public DateTime? ContractTerm { get; set; }
    public DateTime? DateBeginWork { get; set; }
    public DateTime? DateEndWork { get; set; }
    public string? Сurrency { get; set; }
    public decimal? ContractPrice { get; set; }
    public string? NameObject { get; set; }
    public string? FundingSource { get; set; }
    //public bool? IsSubContract { get; set; }
    public bool? IsEngineering { get; set; }
    //public bool? IsAgreementContract { get; set; }
    public string? PaymentСonditionsAvans { get; set; }
    public string? PaymentСonditionsRaschet { get; set; }
    //public double? PaymentСonditionsPrice { get; set; }
    public string? WorkType { get; set; }
    //public bool IsMultiple { get; set; }
    //public int? MultipleContractId { get; set; }
    //public bool IsOneOfMultiple { get; set; }
    public string? Author { get; set; }
    public string? Owner { get; set; }
    public bool IsExpired { get; set; }
    public bool IsClosed { get; set; }
    public bool IsArchive { get; set; }
    public decimal? ThisYearSum { get; set; }
    public decimal? PreYearSum { get; set; }
    public decimal? RemainingSum { get; set; }
    //public DateTime? ArchivedDate { get; set; }
    public string? WorkflowRef { get; set; }
    public string? ProcedureName { get; set; }
    public int? ProcedureId { get; set; }

    public string? SignatoryEmp { get; set; }

    public string? ResponsibleEmp { get; set; }

    public string? GenContractor { get; set; }

    public string? ResponsibleForWork { get; set; }
    public string? Client { get; set; }
}
