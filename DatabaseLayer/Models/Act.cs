using System;
using System.Collections.Generic;

namespace DatabaseLayer.Models;

/// <summary>
/// Акты приостановки/возобновления работ
/// </summary>
public partial class Act
{
    public int Id { get; set; }

    /// <summary>
    /// причина
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// дата акта
    /// </summary>
    public DateTime? DateAct { get; set; }

    /// <summary>
    /// приостановлено с
    /// </summary>
    public DateTime? DateSuspendedFrom { get; set; }

    /// <summary>
    /// приостановлено по
    /// </summary>
    public DateTime? DateSuspendedUntil { get; set; }

    /// <summary>
    /// дата возобновления
    /// </summary>
    public DateTime? DateRenewal { get; set; }

    /// <summary>
    /// приостановлено?
    /// </summary>
    public bool? IsSuspension { get; set; }

    public int? ContractId { get; set; }

    public virtual Contract? Contract { get; set; }

    public virtual ICollection<File> Files { get; set; } = new List<File>();
}
