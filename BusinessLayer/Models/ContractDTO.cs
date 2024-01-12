using DatabaseLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Models
{
    public class ContractDTO
    {
        public int Id { get; set; }

        /// <summary>
        /// Номер договора
        /// </summary>
        public string? Number { get; set; }

        /// <summary>
        /// Ссылка на договоро (если субподряд)
        /// </summary>
        public int? SubContractId { get; set; }

        public int? AgreementContractId { get; set; }

        /// <summary>
        /// Дата договора
        /// </summary>
        public DateTime? Date { get; set; }

        /// <summary>
        /// Срок ввода
        /// </summary>
        public DateTime? EnteringTerm { get; set; }

        /// <summary>
        /// Срок действия договора
        /// </summary>
        public DateTime? ContractTerm { get; set; }

        /// <summary>
        /// Начало работ
        /// </summary>
        public DateTime? DateBeginWork { get; set; }

        /// <summary>
        /// Конец работ
        /// </summary>
        public DateTime? DateEndWork { get; set; }

        /// <summary>
        /// Валюта
        /// </summary>
        public string? Сurrency { get; set; }

        /// <summary>
        /// Цена контракта
        /// </summary>
        public decimal? ContractPrice { get; set; }

        /// <summary>
        /// Цена СМР
        /// </summary>
        public string? NameObject { get; set; }

        /// <summary>
        /// источник финансирования
        /// </summary>
        public string? FundingSource { get; set; }

        /// <summary>
        /// Флаг, является ли договором субподряда
        /// </summary>
        public bool? IsSubContract { get; set; }

        /// <summary>
        /// является ли договор инжиниринговыми услугами
        /// </summary>
        public bool? IsEngineering { get; set; }

        /// <summary>
        /// является ли соглашением с филиалом
        /// </summary>
        public bool? IsAgreementContract { get; set; }

        /// <summary>
        /// условия оплаты (авансы)
        /// </summary>
        public string? PaymentСonditionsAvans { get; set; }

        /// <summary>
        /// условия оплаты (расчеты за выполненные работы)
        /// </summary>
        public string? PaymentСonditionsRaschet { get; set; }

        /// <summary>
        /// условия формирования договорной цены, % (Инжиниронговые услуги)
        /// </summary>
        public double? PaymentСonditionsPrice { get; set; }

        public bool IsMultiple { get; set; }
        public int? MultipleContractId { get; set; }
        public bool IsOneOfMultiple { get; set; }

        public string? Author { get; set; }
        public string? Owner { get; set; }
        public bool IsExpired { get; set; }
        public bool IsClosed { get; set; }
        public bool IsArchive { get; set; }
        public decimal? ThisYearSum { get; set; }
        public decimal? PreYearSum { get; set; }
        public decimal? RemainingSum { get; set; }

        public virtual ContractDTO MultipleContract { get; set; }

        public List<ActDTO> Acts { get; set; } = new List<ActDTO>();

        public virtual ContractDTO? AgreementContract { get; set; }

        public List<AmendmentDTO> Amendments { get; set; } = new List<AmendmentDTO>();

        public List<ContractOrganizationDTO> ContractOrganizations { get; set; } = new List<ContractOrganizationDTO>();

        public List<CorrespondenceDTO> Correspondences { get; set; } = new List<CorrespondenceDTO>();

        public virtual List<EmployeeContractDTO> EmployeeContracts { get; set; } = new List<EmployeeContractDTO>();

        public List<EstimateDocDTO> EstimateDocs { get; set; } = new List<EstimateDocDTO>();

        public List<FormDTO> FormC3as { get; set; } = new List<FormDTO>();
        public List<ContractDTO> InverseMultipleContract { get; set; } = new List<ContractDTO>();
        public List<ContractDTO> InverseAgreementContract { get; set; } = new List<ContractDTO>();

        public List<SelectionProcedureDTO> SelectionProcedures { get; set; } = new List<SelectionProcedureDTO>();

        public List<ContractDTO> InverseSubContract { get; set; } = new List<ContractDTO>();

        public List<MaterialDTO> MaterialGcs { get; set; } = new List<MaterialDTO>();

        public List<PaymentDTO> Payments { get; set; } = new List<PaymentDTO>();

        public List<PrepaymentDTO> Prepayments { get; set; } = new List<PrepaymentDTO>();

        public List<ScopeWorkDTO> ScopeWorks { get; set; } = new List<ScopeWorkDTO>();

        public List<ServiceGCDTO> ServiceGcs { get; set; } = new List<ServiceGCDTO>();

        public virtual ContractDTO? SubContract { get; set; }

        public List<TypeWorkContractDTO> TypeWorkContracts { get; set; } = new List<TypeWorkContractDTO>();

        public List<CommissionActDTO> CommissionActs { get; set; } = new List<CommissionActDTO>();
        public virtual List<ContractFileDTO> ContractFiles { get; set; } = new List<ContractFileDTO>();
    }
}