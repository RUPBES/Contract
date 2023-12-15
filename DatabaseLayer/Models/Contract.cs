using System;
using System.Collections.Generic;

#nullable disable

namespace DatabaseLayer.Models
{
    public partial class Contract
    {
        public int Id { get; set; }
        public string Number { get; set; }
        public int? SubContractId { get; set; }
        public int? AgreementContractId { get; set; }
        public DateTime? Date { get; set; }
        public DateTime? EnteringTerm { get; set; }
        public DateTime? ContractTerm { get; set; }
        public DateTime? DateBeginWork { get; set; }
        public DateTime? DateEndWork { get; set; }
        public string Сurrency { get; set; }
        public decimal? ContractPrice { get; set; }
        public string NameObject { get; set; }
        public string FundingSource { get; set; }
        public bool? IsSubContract { get; set; }
        public bool? IsEngineering { get; set; }
        public bool? IsAgreementContract { get; set; }
        public string PaymentСonditionsAvans { get; set; }
        public string PaymentСonditionsRaschet { get; set; }
        public double? PaymentСonditionsPrice { get; set; }

        public bool IsMultiple { get; set; }
        public int? MultipleContractId { get; set; }
        public bool IsOneOfMultiple { get; set; }
        public virtual Contract MultipleContract { get; set; }

        public virtual Contract AgreementContract { get; set; }
        public virtual Contract SubContract { get; set; }        
        public virtual List<Act> Acts { get; set; } = new List<Act>();
        public virtual List<Amendment> Amendments { get; set; } = new List<Amendment>();
        public virtual List<ContractOrganization> ContractOrganizations { get; set; } = new List<ContractOrganization>();
        public virtual List<Correspondence> Correspondences { get; set; } = new List<Correspondence>();
        public virtual List<EmployeeContract> EmployeeContracts { get; set; } = new List<EmployeeContract>();
        public virtual List<EstimateDoc> EstimateDocs { get; set; } = new List<EstimateDoc>();
        public virtual List<FormC3a> FormC3as { get; set; } = new List<FormC3a>();
        public virtual List<Contract> InverseAgreementContract { get; set; } = new List<Contract>();
        public virtual List<Contract> InverseSubContract { get; set; } = new List<Contract>();
        public virtual List<Contract> InverseMultipleContract { get; set; } = new List<Contract>();
        public virtual List<MaterialGc> MaterialGcs { get; set; } = new List<MaterialGc>();
        public virtual List<Payment> Payments { get; set; } = new List<Payment>();
        public virtual List<Prepayment> Prepayments { get; set; } = new List<Prepayment>();
        public virtual List<ScopeWork> ScopeWorks { get; set; } = new List<ScopeWork>();
        public virtual List<SelectionProcedure> SelectionProcedures { get; set; } = new List<SelectionProcedure>();
        public virtual List<ServiceGc> ServiceGcs { get; set; } = new List<ServiceGc>();
        public virtual List<TypeWorkContract> TypeWorkContracts { get; set; } = new List<TypeWorkContract>();
        public virtual List<CommissionAct> CommissionActs { get; set; } = new List<CommissionAct>();
        public virtual List<ContractFile> ContractFiles { get; set; } = new List<ContractFile>();
    }
}
