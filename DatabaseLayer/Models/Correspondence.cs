using System;
using System.Collections.Generic;

namespace DatabaseLayer.Models;

/// <summary>
/// Переписка с заказчиком
/// </summary>
public partial class Correspondence
{
    public int Id { get; set; }

    /// <summary>
    /// Дата письма
    /// </summary>
    public DateTime? Date { get; set; }

    /// <summary>
    /// Номер письма
    /// </summary>
    public string? Number { get; set; }

    /// <summary>
    /// Краткое содержание
    /// </summary>
    public string? Summary { get; set; }

    /// <summary>
    /// Входящее / Исходящее
    /// </summary>
    public bool? IsInBox { get; set; }

    /// <summary>
    /// Контракт
    /// </summary>
    public int? ContractId { get; set; }

    public virtual Contract? Contract { get; set; }

    public virtual ICollection<File> Files { get; set; } = new List<File>();
}
