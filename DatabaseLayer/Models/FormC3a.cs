using System;
using System.Collections.Generic;

namespace DatabaseLayer.Models;

/// <summary>
/// справки о стоимости выполненных  работ (С-3а)
/// </summary>
public partial class FormC3a
{
    public int Id { get; set; }

    /// <summary>
    /// За какой месяц выполнены работы
    /// </summary>
    public DateTime? Period { get; set; }

    /// <summary>
    /// Дата документа
    /// </summary>
    public DateTime? DateSigning { get; set; }

    /// <summary>
    /// Объем выполненных работ
    /// </summary>
    public decimal? TotalCost { get; set; }

    /// <summary>
    /// стоимость СМР
    /// </summary>
    public decimal? SmrCost { get; set; }

    /// <summary>
    /// стоимость  ПНР
    /// </summary>
    public decimal? PnrCost { get; set; }

    /// <summary>
    /// стоимость оборудования
    /// </summary>
    public decimal? EquipmentCost { get; set; }

    /// <summary>
    /// стоимость остальных работ
    /// </summary>
    public decimal? OtherExpensesCost { get; set; }

    /// <summary>
    /// стоимость доп. работ
    /// </summary>
    public decimal? AdditionalCost { get; set; }

    /// <summary>
    /// стоимость материалов (заказчика)
    /// </summary>
    public decimal? MaterialCost { get; set; }

    /// <summary>
    /// стоимость ген.услуг
    /// </summary>
    public decimal? GenServiceCost { get; set; }

    public string? Number { get; set; }

    /// <summary>
    /// работы проводятся собственными силами?
    /// </summary>
    public bool? IsOwnForces { get; set; }

    /// <summary>
    /// Ссылка на договор
    /// </summary>
    public int? ContractId { get; set; }

    public virtual Contract? Contract { get; set; }
}
