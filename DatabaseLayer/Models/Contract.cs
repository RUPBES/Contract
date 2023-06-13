using System;
using System.Collections.Generic;

namespace DatabaseLayer.Models;

/// <summary>
/// Договор (субподряда)
/// </summary>
public partial class Contract
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
    /// Заказчик
    /// </summary>
    public string? Client { get; set; }

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

    public virtual ICollection<Act> Acts { get; set; } = new List<Act>();

    public virtual Contract? AgreementContract { get; set; }

    public virtual ICollection<Amendment> Amendments { get; set; } = new List<Amendment>();

    public virtual ICollection<ContractOrganization> ContractOrganizations { get; set; } = new List<ContractOrganization>();

    public virtual ICollection<Correspondence> Correspondences { get; set; } = new List<Correspondence>();

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();

    public virtual ICollection<EstimateDoc> EstimateDocs { get; set; } = new List<EstimateDoc>();

    public virtual ICollection<FormC3a> FormC3as { get; set; } = new List<FormC3a>();

    public virtual ICollection<Contract> InverseAgreementContract { get; set; } = new List<Contract>();

    public virtual ICollection<Contract> InverseSubContract { get; set; } = new List<Contract>();

    public virtual ICollection<MaterialGc> MaterialGcs { get; set; } = new List<MaterialGc>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<Prepayment> Prepayments { get; set; } = new List<Prepayment>();

    public virtual ICollection<ScopeWork> ScopeWorks { get; set; } = new List<ScopeWork>();

    public virtual ICollection<SelectionProcedure> SelectionProcedures { get; set; } = new List<SelectionProcedure>();

    public virtual ICollection<ServiceGc> ServiceGcs { get; set; } = new List<ServiceGc>();

    public virtual Contract? SubContract { get; set; }

    public virtual ICollection<TypeWorkContract> TypeWorkContracts { get; set; } = new List<TypeWorkContract>();

    public virtual ICollection<СommissionAct> СommissionActs { get; set; } = new List<СommissionAct>();
}
