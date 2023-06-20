using System;
using System.Collections.Generic;

namespace DatabaseLayer.Models;

/// <summary>
/// Связь &quot;Организации&quot; и &quot;Контракта&quot;
/// </summary>
public partial class ContractOrganization
{
    public int OrganizationId { get; set; }

    public int ContactId { get; set; }

    /// <summary>
    /// ген.подрядчик?
    /// </summary>
    public bool? IsGenContractor { get; set; }

    /// <summary>
    /// Заказчик?
    /// </summary>
    public bool? IsClient { get; set; }

    public virtual Contract Contact { get; set; } = null!;

    public virtual Organization Organization { get; set; } = null!;
}
