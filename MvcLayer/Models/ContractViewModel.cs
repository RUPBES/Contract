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
        /// Ссылка на договоро (если субподряд)
        /// </summary>
        public int? SubContractId { get; set; }

        public int? AgreementContractId { get; set; }

        /// <summary>
        /// Дата договора
        /// </summary>
          [DisplayName("Дата договора")]
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
        [DisplayName("Срок выполнения/Начало работ")]
        public DateTime? DateBeginWork { get; set; }

        /// <summary>
        /// Конец работ
        /// </summary>
        [DisplayName("Срок выполнения/Окончание работ")]
        public DateTime? DateEndWork { get; set; }

        /// <summary>
        /// Валюта
        /// </summary>
        [DisplayName("Валюта")]

        public string? Сurrency { get; set; }

        /// <summary>
        /// Цена контракта
        /// </summary>
        [DisplayName("Цена контракта")]
        public decimal? ContractPrice { get; set; }

        /// <summary>
        /// Цена СМР
        /// </summary>
        [DisplayName("Наименование объекта")]
        public string? NameObject { get; set; }

        /// <summary>
        /// Заказчик
        /// </summary>
        /// 
        [DisplayName("Заказчик")]
        public string? Client { get; set; }

        /// <summary>
        /// источник финансирования
        /// </summary>
        /// 
        [DisplayName("Источник финансирования")]
        public string? FundingSource { get; set; }

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
        public bool? IsEngineering { get; set; }

        /// <summary>
        /// является ли соглашением с филиалом
        /// </summary>
        [DisplayName("Соглашением с филиалом?")]
        public bool? IsAgreementContract { get; set; }

        public List<ActDTO> Acts { get; set; } = new List<ActDTO>();

        public virtual ContractViewModel? AgreementContract { get; set; }

        public List<AmendmentDTO> Amendments { get; set; } = new List<AmendmentDTO>();

        public List<ContractOrganizationDTO> ContractOrganizations { get; set; } = new List<ContractOrganizationDTO>();

        public List<CorrespondenceDTO> Correspondences { get; set; } = new List<CorrespondenceDTO>();

        public List<EmployeeViewModel> Employees { get; set; } = new List<EmployeeViewModel>();

        //public List<EstimateDocDTO> EstimateDocs { get; set; } = new List<EstimateDocDTO>();

        //public List<FormC3aDTO> FormC3as { get; set; } = new List<FormC3aDTO>();

        public List<ContractViewModel> InverseAgreementContract { get; set; } = new List<ContractViewModel>();

        public List<ContractViewModel> InverseSubContract { get; set; } = new List<ContractViewModel>();

        //public List<MaterialGcDTO> MaterialGcs { get; set; } = new List<MaterialGcDTO>();

        //public List<PaymentDTO> Payments { get; set; } = new List<PaymentDTO>();

        //public List<PrepaymentDTO> Prepayments { get; set; } = new List<PrepaymentDTO>();

        //public List<ScopeWorkDTO> ScopeWorks { get; set; } = new List<ScopeWorkDTO>();

        //public List<SelectionProcedureDTO> SelectionProcedures { get; set; } = new List<SelectionProcedureDTO>();

        //public List<ServiceGcDTO> ServiceGcs { get; set; } = new List<ServiceGcDTO>();

        public virtual ContractViewModel? SubContract { get; set; }

        //public List<TypeWorkContractDTO> TypeWorkContracts { get; set; } = new List<TypeWorkContractDTO>();

        //public List<СommissionActDTO> СommissionActs { get; set; } = new List<СommissionActDTO>();
    }
}
