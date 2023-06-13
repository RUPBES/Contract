using System;
using System.Collections.Generic;

namespace DatabaseLayer.Models;

/// <summary>
/// Объем работ
/// </summary>
public partial class ScopeWork
{
    public int Id { get; set; }

    /// <summary>
    /// Период отчета
    /// </summary>
    public DateTime? Period { get; set; }

    /// <summary>
    /// стоимость работ без НДС (согласно договору)
    /// </summary>
    public decimal? CostNoNds { get; set; }

    /// <summary>
    /// согласно графику производства работ по договору
    /// </summary>
    public decimal? CostNds { get; set; }

    /// <summary>
    /// стоимость СМР
    /// </summary>
    public decimal? SmrCost { get; set; }

    /// <summary>
    /// Цена ПНР
    /// </summary>
    public decimal? PnrCost { get; set; }

    /// <summary>
    /// Цена оборудования
    /// </summary>
    public decimal? EquipmentCost { get; set; }

    /// <summary>
    /// Цена остальных работ
    /// </summary>
    public decimal? OtherExpensesCost { get; set; }

    /// <summary>
    /// Цена дополнительных работ
    /// </summary>
    public decimal? AdditionalCost { get; set; }

    /// <summary>
    /// Цена материалов 
    /// </summary>
    public decimal? MaterialCost { get; set; }

    public decimal? GenServiceCost { get; set; }

    /// <summary>
    /// работы проводятся собственными силами?
    /// </summary>
    public bool? IsOwnForces { get; set; }

    /// <summary>
    /// Контракт
    /// </summary>
    public int? ContractId { get; set; }

    /// <summary>
    /// изменено?
    /// </summary>
    public bool? IsChange { get; set; }

    /// <summary>
    /// ID измененного объема работ
    /// </summary>
    public int? ChangeScopeWorkId { get; set; }

    public virtual ScopeWork? ChangeScopeWork { get; set; }

    public virtual Contract? Contract { get; set; }

    public virtual ICollection<ScopeWork> InverseChangeScopeWork { get; set; } = new List<ScopeWork>();

    public virtual ICollection<Amendment> Amendments { get; set; } = new List<Amendment>();
}
