using System;
using System.Collections.Generic;

namespace DatabaseLayer.Models;

/// <summary>
/// Авансовые платежи планируемые
/// </summary>
public partial class PrepaymentPlan
{
    public int Id { get; set; }

    public DateTime? Period { get; set; }

    public decimal? CurrentValue { get; set; }

    public decimal? TargetValue { get; set; }

    public decimal? WorkingOutValue { get; set; }

    public int? PrepaymentId { get; set; }

    public virtual Prepayment? Prepayment { get; set; }
}
