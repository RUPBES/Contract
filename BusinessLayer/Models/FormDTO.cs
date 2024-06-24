namespace BusinessLayer.Models
{
    public class FormDTO
    {
        public int Id { get; set; }
        public DateTime? Period { get; set; }
        public DateTime? DateSigning { get; set; }

        public decimal? TotalCost { get; set; }
        public decimal? TotalCostToBePaid { get; set; }

        public decimal? SmrCost { get; set; }
        public decimal? SmrContractCost { get; set; }
        public decimal? SmrNdsCost { get; set; }

        public decimal? PnrCost { get; set; }
        public decimal? PnrContractCost { get; set; }
        public decimal? PnrNdsCost { get; set; }

        public decimal? EquipmentCost { get; set; }
        public decimal? EquipmentContractCost { get; set; }
        public decimal? EquipmentNdsCost { get; set; }
        public decimal? EquipmentClientCost { get; set; } //стоимость оборудования заказчика (справочно)

        public decimal? AdditionalCost { get; set; }
        public decimal? AdditionalContractCost { get; set; }
        public decimal? AdditionalNdsCost { get; set; }

        public decimal? OtherExpensesCost { get; set; }
        public decimal? OtherExpensesNdsCost { get; set; }

        public decimal? MaterialCost { get; set; }
        public decimal? MaterialClientCost { get; set; } //стоимость материалов (заказчика)

        public decimal? GenServiceCost { get; set; }

        public decimal? OffsetTargetPrepayment { get; set; }
        public decimal? OffsetCurrentPrepayment { get; set; }

        public bool? IsExemptFromVAT { get; set; } //освобожден от уплаты ндс?
        public bool? IsOwnForces { get; set; }     //собственными силами(у генподрядчика есть с пометкой - true ) - по умолчанию false   
        public int? ContractId { get; set; }

        public decimal? CostToConstructionIndustryFund { get; set; } //отчисления в фонд строительной отрасли
        public decimal? СostStatisticReportOfContractor { get; set; } //стоимость работ для статистической отчетности подрядчика (слравочно)        
    }
}
