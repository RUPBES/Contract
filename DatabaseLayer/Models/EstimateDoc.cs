using System;
using System.Collections.Generic;

namespace DatabaseLayer.Models;

/// <summary>
/// Проектно-сметная документация
/// </summary>
public partial class EstimateDoc
{
    public int Id { get; set; }

    /// <summary>
    /// № п/п
    /// </summary>
    public string? Number { get; set; }

    /// <summary>
    /// Дата изменения в проектно-сметную документацию
    /// </summary>
    public DateTime? DateChange { get; set; }

    /// <summary>
    /// Дата выхода смет
    /// </summary>
    public DateTime? DateOutput { get; set; }

    /// <summary>
    /// Причины изменения проектно-сметной документации
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// Контракт
    /// </summary>
    public int? ContractId { get; set; }

    /// <summary>
    /// Проверка: изменения / оригинал
    /// </summary>
    public bool? IsChange { get; set; }

    public virtual Contract? Contract { get; set; }

    public virtual ICollection<File> Files { get; set; } = new List<File>();
}
