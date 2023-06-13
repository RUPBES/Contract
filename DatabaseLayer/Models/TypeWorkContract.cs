using System;
using System.Collections.Generic;

namespace DatabaseLayer.Models;

/// <summary>
/// вид работ - договор
/// </summary>
public partial class TypeWorkContract
{
    /// <summary>
    /// Ссылка на типовые работы
    /// </summary>
    public int TypeWorkId { get; set; }

    /// <summary>
    /// Контракт
    /// </summary>
    public int ContractId { get; set; }

    /// <summary>
    /// Название работ
    /// </summary>
    public string? AdditionalName { get; set; }

    public virtual Contract Contract { get; set; } = null!;

    public virtual TypeWork TypeWork { get; set; } = null!;
}
