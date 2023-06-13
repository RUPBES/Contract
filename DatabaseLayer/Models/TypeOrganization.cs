using System;
using System.Collections.Generic;

namespace DatabaseLayer.Models;

/// <summary>
/// 1 - заказчик, 2 - получатель контракта.
/// </summary>
public partial class TypeOrganization
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<ContractOrganization> ContractOrganizations { get; set; } = new List<ContractOrganization>();
}
