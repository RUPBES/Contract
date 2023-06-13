using System;
using System.Collections.Generic;

namespace DatabaseLayer.Models;

/// <summary>
/// Справочник стандартных работ
/// </summary>
public partial class TypeWork
{
    public int Id { get; set; }

    /// <summary>
    /// Название работ
    /// </summary>
    public string? Name { get; set; }

    public virtual ICollection<TypeWorkContract> TypeWorkContracts { get; set; } = new List<TypeWorkContract>();
}
