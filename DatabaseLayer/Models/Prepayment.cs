using System;
using System.Collections.Generic;

namespace DatabaseLayer.Models;

/// <summary>
/// Аванс
/// </summary>
public partial class Prepayment
{
    public int Id { get; set; }

    /// <summary>
    /// Текущие авансы
    /// </summary>
    public decimal? CurrentValue { get; set; }

    /// <summary>
    /// Текущие авансы по факту
    /// </summary>
    public decimal? CurrentValueFact { get; set; }

    /// <summary>
    /// Целевые Авансы
    /// </summary>
    public decimal? TargetValue { get; set; }

    /// <summary>
    /// Целевые Авансы по факту
    /// </summary>
    public decimal? TargetValueFact { get; set; }

    /// <summary>
    /// Отработка целевых
    /// </summary>
    public decimal? WorkingOutValue { get; set; }

    /// <summary>
    /// Отработка целевых фактическое
    /// </summary>
    public decimal? WorkingOutValueFact { get; set; }

    /// <summary>
    /// Месяц за который получено
    /// </summary>
    public DateTime? Period { get; set; }

    /// <summary>
    /// Контракт
    /// </summary>
    public int? ContractId { get; set; }

    /// <summary>
    /// Изменено?
    /// </summary>
    public bool? IsChange { get; set; }

    /// <summary>
    /// ID измененного аванса
    /// </summary>
    public int? ChangePrepaymentId { get; set; }

    public virtual Prepayment? ChangePrepayment { get; set; }

    public virtual Contract? Contract { get; set; }

    public virtual ICollection<Prepayment> InverseChangePrepayment { get; set; } = new List<Prepayment>();

    public virtual ICollection<Amendment> Amendments { get; set; } = new List<Amendment>();
}
