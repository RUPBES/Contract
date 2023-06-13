using System;
using System.Collections.Generic;

namespace DatabaseLayer.Models;

/// <summary>
/// Услуги генподряда
/// </summary>
public partial class ServiceGc
{
    public int Id { get; set; }

    /// <summary>
    /// процент услуг
    /// </summary>
    public int? ServicePercent { get; set; }

    /// <summary>
    /// Месяц и год
    /// </summary>
    public DateTime? Period { get; set; }

    /// <summary>
    /// Сумма по договору
    /// </summary>
    public decimal? Price { get; set; }

    /// <summary>
    /// Сумма фактическая
    /// </summary>
    public decimal? FactPrice { get; set; }

    /// <summary>
    /// Контракт
    /// </summary>
    public int? ContractId { get; set; }

    /// <summary>
    /// изменено?
    /// </summary>
    public bool? IsChange { get; set; }

    /// <summary>
    /// ID измененной услуги
    /// </summary>
    public int? ChangeServiceId { get; set; }

    public virtual ServiceGc? ChangeService { get; set; }

    public virtual Contract? Contract { get; set; }

    public virtual ICollection<ServiceGc> InverseChangeService { get; set; } = new List<ServiceGc>();

    public virtual ICollection<Amendment> Amendments { get; set; } = new List<Amendment>();
}
