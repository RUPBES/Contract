using System;
using System.Collections.Generic;

namespace DatabaseLayer.Models;

/// <summary>
/// акт ввода
/// </summary>
public partial class СommissionAct
{
    public int Id { get; set; }

    public string? Number { get; set; }

    /// <summary>
    /// Дата акта ввода
    /// </summary>
    public DateTime? Date { get; set; }

    public int? ContractId { get; set; }

    public virtual Contract? Contract { get; set; }

    public virtual ICollection<File> Files { get; set; } = new List<File>();
}
