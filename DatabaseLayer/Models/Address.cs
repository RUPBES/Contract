using System;
using System.Collections.Generic;

namespace DatabaseLayer.Models;

/// <summary>
/// Юр. адрес
/// </summary>
public partial class Address
{
    public int Id { get; set; }

    /// <summary>
    /// юр. адрес организации
    /// </summary>
    public string? FullAddress { get; set; }

    /// <summary>
    /// Почтовый индекс
    /// </summary>
    public string? PostIndex { get; set; }

    public int? OrganizationId { get; set; }

    public virtual Organization? Organization { get; set; }
}
