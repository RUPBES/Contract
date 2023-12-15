using BusinessLayer.Models;
using System.ComponentModel;

namespace MvcLayer.Models
{
    public class ContractViewModel
    {

        public int Id { get; set; }

        /// <summary>
        /// Номер договора
        /// </summary>
        /// 
        [DisplayName("Номер")]
        public string? Number { get; set; }

        /// <summary>
        /// Ссылка на договор (если субподряд)
        /// </summary>
        public int? SubContractId { get; set; }

        /// <summary>
        /// Ссылка на договор (если соглашение с филиалом)
        /// </summary>
        public int? AgreementContractId { get; set; }

        /// <summary>
        /// Дата договора
        /// </summary>
        [DisplayName("Дата заключения договора")]
        public DateTime? Date { get; set; }

        /// <summary>
        /// Срок ввода
        /// </summary>
        [DisplayName("Срок ввода")]
        public DateTime? EnteringTerm { get; set; }

        /// <summary>
        /// Срок действия договора
        /// </summary>
        [DisplayName("Срок действия")]
        public DateTime? ContractTerm { get; set; }

        /// <summary>
        /// Начало работ
        /// </summary>
        [DisplayName("Начало работ")]
        public DateTime? DateBeginWork { get; set; }

        /// <summary>
        /// Конец работ
        /// </summary>
        [DisplayName("Окончание работ")]
        public DateTime? DateEndWork { get; set; }

        /// <summary>
        /// Валюта
        /// </summary>
        [DisplayName("Валюта")]
        public string? Сurrency { get; set; }

        /// <summary>
        /// Цена контракта
        /// </summary>
        [DisplayName("Договорная цена")]
        public decimal? ContractPrice { get; set; }

        /// <summary>
        /// Наименование объекта
        /// </summary>
        [DisplayName("Наименование объекта")]
        public string? NameObject { get; set; }


        /// <summary>
        /// источник финансирования
        /// </summary>
        /// 
        [DisplayName("Источник финансирования")]
        public string? FundingSource { get; set; }

        public List<string> FundingFS { get; set; } = new List<string>();

        /// <summary>
        /// Флаг, является ли договором субподряда
        /// </summary>
        /// 
        [DisplayName("Договор субподряда?")]
        public bool? IsSubContract { get; set; }

        /// <summary>
        /// является ли договор инжиниринговыми услугами
        /// </summary>
        [DisplayName("Договор инжиниринговых услуг?")]
        public bool IsEngineering { get; set; }

        /// <summary>
        /// является ли соглашением с филиалом
        /// </summary>
        [DisplayName("Соглашением с филиалом?")]
        public bool? IsAgreementContract { get; set; }

        public List<string> PaymentCA { get; set; } = new List<string>();

        /// <summary>
        /// условия оплаты (авансы)
        /// </summary>
        [DisplayName("Условия авансирования")]
        public string? PaymentСonditionsAvans { get; set; }

        /// <summary>
        /// условия формирования договорной цены, % (Инжиниронговые услуги)
        /// </summary>
        [DisplayName("Условия договорной цены, %")]
        public double? PaymentСonditionsPrice { get; set; }

        /// <summary>
        /// условия оплаты (расчеты за выполненные работы)
        /// </summary>
        [DisplayName("Расчет за выполненные работы")]
        public string? PaymentСonditionsRaschet { get; set; }
        public int? PaymentСonditionsDaysRaschet { get; set; }
        public int? PaymentСonditionsDaysRaschet2 { get; set; }
        public int? ContractType { get; set; }

        public bool IsMultiple { get; set; }
        public int? MultipleContractId { get; set; }
        public bool IsOneOfMultiple { get; set; }
        public ContractViewModel? MultipleContract { get; set; }

        public List<ActDTO> Acts { get; set; } = new List<ActDTO>();

        public ContractViewModel? AgreementContract { get; set; }

        public List<AmendmentDTO> Amendments { get; set; } = new List<AmendmentDTO>();

        public List<ContractOrganizationDTO> ContractOrganizations { get; set; } = new List<ContractOrganizationDTO>();

        public List<CorrespondenceDTO> Correspondences { get; set; } = new List<CorrespondenceDTO>();

        public List<EmployeeContractDTO> EmployeeContracts { get; set; } = new List<EmployeeContractDTO>();

        public List<EstimateDocDTO> EstimateDocs { get; set; } = new List<EstimateDocDTO>();

        public List<FormDTO> FormC3as { get; set; } = new List<FormDTO>();
        public List<SelectionProcedureDTO> SelectionProcedures { get; set; } = new List<SelectionProcedureDTO>();
        public List<ContractViewModel> InverseAgreementContract { get; set; } = new List<ContractViewModel>();
        public List<ContractViewModel> InverseMultipleContract { get; set; } = new List<ContractViewModel>();
        public List<ContractViewModel> InverseSubContract { get; set; } = new List<ContractViewModel>();

        public List<MaterialDTO> MaterialGcs { get; set; } = new List<MaterialDTO>();

        public List<PaymentDTO> Payments { get; set; } = new List<PaymentDTO>();

        public List<PrepaymentDTO> Prepayments { get; set; } = new List<PrepaymentDTO>();

        public List<ScopeWorkDTO> ScopeWorks { get; set; } = new List<ScopeWorkDTO>();

        public List<ServiceGCDTO> ServiceGcs { get; set; } = new List<ServiceGCDTO>();

        public ContractViewModel? SubContract { get; set; }

        public List<TypeWorkContractDTO> TypeWorkContracts { get; set; } = new List<TypeWorkContractDTO>();

        public List<CommissionActDTO> СommissionActs { get; set; } = new List<CommissionActDTO>();
        public List<ContractFileDTO> ContractFiles { get; set; } = new List<ContractFileDTO>();
    }
}
