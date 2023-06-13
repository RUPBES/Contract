using System;
using System.Collections.Generic;

namespace DatabaseLayer.Models;

/// <summary>
/// денежные средства, подлежащие оплате
/// </summary>
public partial class Payment
{
    public int Id { get; set; }

    /// <summary>
    /// всего к оплате
    /// </summary>
    public decimal? PaySum { get; set; }

    /// <summary>
    /// из них на счет РУП &quot;БЭС&quot;-УКХ&quot;
    /// </summary>
    public decimal? PaySumForRupBes { get; set; }

    public DateTime? Period { get; set; }

    public int? ContractId { get; set; }

    public virtual Contract? Contract { get; set; }
}
