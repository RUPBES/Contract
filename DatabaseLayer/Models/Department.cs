using System;
using System.Collections.Generic;

namespace DatabaseLayer.Models;

/// <summary>
/// Отдел/управление
/// </summary>
public partial class Department
{
    public int Id { get; set; }

    /// <summary>
    /// Название
    /// </summary>
    public string? Name { get; set; }

    public int? OrganizationId { get; set; }

    public virtual Organization? Organization { get; set; }

    public virtual List<Employee> Employees { get; set; } = new List<Employee>();
}
