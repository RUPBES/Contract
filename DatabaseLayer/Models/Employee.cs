using System;
using System.Collections.Generic;

namespace DatabaseLayer.Models;

/// <summary>
/// Сотрудники
/// </summary>
public partial class Employee
{
    public int Id { get; set; }

    public string? FullName { get; set; }

    public string? Fio { get; set; }

    public string? Position { get; set; }

    public string? Email { get; set; }

    public int? ContractId { get; set; }

    public virtual Contract? Contract { get; set; }

    public virtual ICollection<Phone> Phones { get; set; } = new List<Phone>();

    public virtual ICollection<Department> Departments { get; set; } = new List<Department>();
}
