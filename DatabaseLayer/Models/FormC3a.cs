﻿using System;
using System.Collections.Generic;

#nullable disable

namespace DatabaseLayer.Models
{
    public partial class FormC3a
    {
        public int Id { get; set; }
        public DateTime? Period { get; set; }
        public DateTime? DateSigning { get; set; }
        public decimal? TotalCost { get; set; }
        public decimal? SmrCost { get; set; }
        public decimal? PnrCost { get; set; }
        public decimal? EquipmentCost { get; set; }
        public decimal? OtherExpensesCost { get; set; }
        public decimal? AdditionalCost { get; set; }
        public decimal? MaterialCost { get; set; }
        public decimal? GenServiceCost { get; set; }
        public decimal? OffsetTargetPrepayment { get; set; }
        public decimal? OffsetCurrentPrepayment { get; set; }
        public decimal? TotalCostToBePaid { get; set; }
        public string Number { get; set; }
        public bool? IsOwnForces { get; set; }
        public bool? IsFinal { get; set; }
        public int? ContractId { get; set; }

        public virtual Contract Contract { get; set; }
        public virtual ICollection<FormFile> FormFiles { get; set; }
    }
}
