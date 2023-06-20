using System;
using System.Collections.Generic;

namespace DatabaseLayer.Models;

/// <summary>
/// Процедура выбора
/// </summary>
public partial class SelectionProcedure
{
    public int Id { get; set; }

    /// <summary>
    /// Название
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Вид закупки
    /// </summary>
    public string? TypeProcedure { get; set; }

    /// <summary>
    /// Срок проведения начало
    /// </summary>
    public DateTime? DateBegin { get; set; }

    /// <summary>
    /// Срок проведения окончание
    /// </summary>
    public DateTime? DateEnd { get; set; }

    /// <summary>
    /// Стартовая цена
    /// </summary>
    public decimal? StartPrice { get; set; }

    /// <summary>
    /// Цена акцента
    /// </summary>
    public decimal? AcceptancePrice { get; set; }

    /// <summary>
    /// Номер акцента
    /// </summary>
    public string? AcceptanceNumber { get; set; }

    /// <summary>
    /// Дата акцента
    /// </summary>
    public DateTime? DateAcceptance { get; set; }

    public int? ContractId { get; set; }

    public virtual Contract? Contract { get; set; }
}
