using System;
using System.Collections.Generic;

namespace DatabaseLayer.Models;

/// <summary>
/// Материалы генподрядчика
/// </summary>
public partial class MaterialGc
{
    public int Id { get; set; }

    /// <summary>
    /// Цена по договору
    /// </summary>
    public decimal? Price { get; set; }

    /// <summary>
    /// Цена фактическая
    /// </summary>
    public decimal? FactPrice { get; set; }

    /// <summary>
    /// период отчета
    /// </summary>
    public DateTime? Period { get; set; }

    /// <summary>
    /// Контракт
    /// </summary>
    public int? ContractId { get; set; }

    /// <summary>
    /// изменено?
    /// </summary>
    public bool? IsChange { get; set; }

    /// <summary>
    /// ID измененных материалов
    /// </summary>
    public int? ChangeMaterialId { get; set; }

    public virtual MaterialGc? ChangeMaterial { get; set; }

    public virtual Contract? Contract { get; set; }

    public virtual ICollection<MaterialGc> InverseChangeMaterial { get; set; } = new List<MaterialGc>();

    public virtual ICollection<Amendment> Amendments { get; set; } = new List<Amendment>();
}
