using System;
using System.Collections.Generic;

namespace DatabaseLayer.Models;

/// <summary>
/// связь сотрудник - контракт
/// </summary>
public partial class EmployeeContract
{
    public int EmployeeId { get; set; }

    public int ContractId { get; set; }

    /// <summary>
    /// подписант договора
    /// </summary>
    public bool? IsSignatory { get; set; }

    /// <summary>
    /// ответственный за ведение договора
    /// </summary>
    public bool? IsResponsible { get; set; }

    public virtual Contract Contract { get; set; } = null!;

    public virtual Employee Employee { get; set; } = null!;
}
