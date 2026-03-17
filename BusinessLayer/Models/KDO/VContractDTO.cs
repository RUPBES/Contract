using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Models.KDO
{
    public class VContractDTO
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
        public string? WorkType { get; set; }
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
        public DateTime? ArchivedDate { get; set; }

        public string? ProcedureName { get; set; }
        public int? ProcedureId { get; set; }

        public string? SignatoryEmp { get; set; }

        public string? ResponsibleEmp { get; set; }

        public string? GenContractor { get; set; }

        public string? ResponsibleForWork { get; set; }
        public string? Client { get; set; }
        public string? WorkflowRef { get; set; }
    }
}
