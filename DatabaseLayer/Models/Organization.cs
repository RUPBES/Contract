using System;
using System.Collections.Generic;

namespace DatabaseLayer.Models;

/// <summary>
/// Организация
/// </summary>
public partial class Organization
{
    public int Id { get; set; }

    /// <summary>
    /// Полное название
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Аббревиатура
    /// </summary>
    public string? Abbr { get; set; }

    /// <summary>
    /// УНП предприятия
    /// </summary>
    public string? Unp { get; set; }

    /// <summary>
    /// электронная почта
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// расчетный счет
    /// </summary>
    public string? PaymentAccount { get; set; }

    public virtual List<Address> Addresses { get; set; } = new List<Address>();

    public virtual List<ContractOrganization> ContractOrganizations { get; set; } = new List<ContractOrganization>();

    public virtual List<Department> Departments { get; set; } = new List<Department>();

    public virtual List<Phone> Phones { get; set; } = new List<Phone>();
}
