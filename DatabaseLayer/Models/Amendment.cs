using System;
using System.Collections.Generic;

namespace DatabaseLayer.Models;

/// <summary>
/// Изменения к договору
/// </summary>
public partial class Amendment
{
    public int Id { get; set; }

    /// <summary>
    /// номер изменений
    /// </summary>
    public string? Number { get; set; }

    /// <summary>
    /// дата изменения
    /// </summary>
    public DateTime? Date { get; set; }

    /// <summary>
    /// Причина
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// договорная (контрактная) цена, руб. с НДС
    /// </summary>
    public decimal? ContractPrice { get; set; }

    /// <summary>
    /// срок выполнения работ (Начало)
    /// </summary>
    public DateTime? DateBeginWork { get; set; }

    /// <summary>
    /// срок выполнения работ (Окончание)
    /// </summary>
    public DateTime? DateEndWork { get; set; }

    /// <summary>
    /// срок ввода объекта в эксплуатацию
    /// </summary>
    public DateTime? DateEntryObject { get; set; }

    /// <summary>
    /// существенные изменения пунктов Договора
    /// </summary>
    public string? ContractChanges { get; set; }

    /// <summary>
    /// коментарии к изменениям
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Договор
    /// </summary>
    public int? ContractId { get; set; }

    public virtual Contract? Contract { get; set; }

    public virtual ICollection<File> Files { get; set; } = new List<File>();

    public virtual ICollection<MaterialGc> Materials { get; set; } = new List<MaterialGc>();

    public virtual ICollection<Prepayment> Prepayments { get; set; } = new List<Prepayment>();

    public virtual ICollection<ScopeWork> ScopeWorks { get; set; } = new List<ScopeWork>();

    public virtual ICollection<ServiceGc> Services { get; set; } = new List<ServiceGc>();
}
