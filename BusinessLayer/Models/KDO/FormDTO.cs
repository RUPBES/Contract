namespace BusinessLayer.Models.KDO
{
    public class FormDTO
    {
        public int Id { get; set; }
        public DateTime? Period { get; set; }
        public DateTime? DateSigning { get; set; }

        public decimal TotalCost { get; set; }
        public decimal TotalNoNdsCost { get; set; } // для таблицы об.работ итог
        public decimal TotalCostToBePaid { get; set; }

        public decimal SmrCost { get; set; }
        public decimal SmrContractCost { get; set; }
        public decimal SmrNdsCost { get; set; }

        public decimal PnrCost { get; set; }
        public decimal PnrContractCost { get; set; }
        public decimal PnrNdsCost { get; set; }

        public decimal EquipmentCost { get; set; }
        public decimal EquipmentContractCost { get; set; }
        public decimal EquipmentNdsCost { get; set; }
        public decimal EquipmentClientCost { get; set; } //стоимость оборудования заказчика (справочно)

        public decimal AdditionalCost { get; set; }
        public decimal AdditionalContractCost { get; set; }
        public decimal AdditionalNdsCost { get; set; }

        public decimal OtherExpensesCost { get; set; }
        public decimal OtherExpensesNdsCost { get; set; }

        public decimal MaterialCost { get; set; }
        public decimal MaterialClientCost { get; set; } //стоимость материалов (заказчика)

        public decimal GenServiceCost { get; set; }

        public decimal OffsetTargetPrepayment { get; set; }
        public decimal OffsetCurrentPrepayment { get; set; }
        public decimal Reserve { get; set; }
        public bool? IsExemptFromVAT { get; set; } //освобожден от уплаты ндс?
        public bool? IsOwnForces { get; set; }     //собственными силами(у генподрядчика есть с пометкой - true ) - по умолчанию false   
        public int? ContractId { get; set; }

        public decimal CostToConstructionIndustryFund { get; set; } //отчисления в фонд строительной отрасли
        public decimal CostStatisticReportOfContractor { get; set; } //стоимость работ для статистической отчетности подрядчика (слравочно)        

        /// <summary>
        /// Добавляет значения первому объекту из второго (ID, Period и др. в результирующем объекте будет равен первому объекту)
        /// </summary>
        /// <param name="firstForm"></param>
        /// <param name="secForm"></param>
        /// <returns>Новый объект равный первому, у котрого добавлены данные стоимостей из второго объекта</returns>
        public static FormDTO operator + (FormDTO? firstForm, FormDTO? secForm)
        {
            return new FormDTO
            {
                Id = firstForm.Id,
                Period = firstForm.Period,
                DateSigning = firstForm.DateSigning,
                IsExemptFromVAT = firstForm.IsExemptFromVAT,
                IsOwnForces = firstForm.IsOwnForces,
                ContractId = firstForm.ContractId,

                PnrCost = (firstForm?.PnrCost ?? 0) + (secForm?.PnrCost ?? 0),
                PnrContractCost = (firstForm?.PnrContractCost ?? 0) + (secForm?.PnrContractCost ?? 0),
                PnrNdsCost = (firstForm?.PnrNdsCost ?? 0) + (secForm?.PnrNdsCost ?? 0),
                SmrCost = (firstForm?.SmrCost ?? 0) + (secForm?.SmrCost ?? 0),
                SmrContractCost = (firstForm?.SmrContractCost ?? 0) + (secForm?.SmrContractCost ?? 0),
                SmrNdsCost = (firstForm?.SmrNdsCost ?? 0) + (secForm?.SmrNdsCost ?? 0),
                EquipmentCost = (firstForm?.EquipmentCost ?? 0) + (secForm?.EquipmentCost ?? 0),
                EquipmentContractCost = (firstForm?.EquipmentContractCost ?? 0) + (secForm?.EquipmentContractCost ?? 0),
                EquipmentNdsCost = (firstForm?.EquipmentNdsCost ?? 0) + (secForm?.EquipmentNdsCost ?? 0),
                EquipmentClientCost = (firstForm?.EquipmentClientCost ?? 0) + (secForm?.EquipmentClientCost ?? 0),
                OtherExpensesCost = (firstForm?.OtherExpensesCost ?? 0) + (secForm?.OtherExpensesCost ?? 0),
                OtherExpensesNdsCost = (firstForm?.OtherExpensesNdsCost ?? 0) + (secForm?.OtherExpensesNdsCost ?? 0),
                AdditionalCost = (firstForm?.AdditionalCost ?? 0) + (secForm?.AdditionalCost ?? 0),
                AdditionalContractCost = (firstForm?.AdditionalContractCost ?? 0) + (secForm?.AdditionalContractCost ?? 0),
                AdditionalNdsCost = (firstForm?.AdditionalNdsCost ?? 0) + (secForm?.AdditionalNdsCost ?? 0),
                GenServiceCost = (firstForm?.GenServiceCost ?? 0) + (secForm?.GenServiceCost ?? 0),
                MaterialCost = (firstForm?.MaterialCost ?? 0) + (secForm?.MaterialCost ?? 0),
                MaterialClientCost = (firstForm?.MaterialClientCost ?? 0) + (secForm?.MaterialClientCost ?? 0),
                Reserve = (firstForm?.Reserve ?? 0) + (secForm?.Reserve ?? 0),
                CostToConstructionIndustryFund = (firstForm?.CostToConstructionIndustryFund ?? 0) + (secForm?.CostToConstructionIndustryFund ?? 0),
                CostStatisticReportOfContractor = (firstForm?.CostStatisticReportOfContractor ?? 0) + (secForm?.CostStatisticReportOfContractor ?? 0),
                OffsetCurrentPrepayment = (firstForm?.OffsetCurrentPrepayment ?? 0) + (secForm?.OffsetCurrentPrepayment ?? 0),
                OffsetTargetPrepayment = (firstForm?.OffsetTargetPrepayment ?? 0) + (secForm?.OffsetTargetPrepayment ?? 0)
            };
        }

        /// <summary>
        /// Вычетает значения у первого объекта значениями из второго (ID, Period и др. в результирующем объекте будетут равны первому объекту)
        /// </summary>
        /// <param name="firstForm"></param>
        /// <param name="secForm"></param>
        /// <returns>Новый объект равный первому, у котрого вычтены данные стоимостей из второго объекта</returns>
        public static FormDTO operator - (FormDTO? firstForm, FormDTO? secForm)
        {
            return new FormDTO
            {
                Id = firstForm.Id,
                Period = firstForm.Period,
                DateSigning = firstForm.DateSigning,
                IsExemptFromVAT = firstForm.IsExemptFromVAT,
                IsOwnForces = firstForm.IsOwnForces,
                ContractId = firstForm.ContractId,

                PnrCost = (firstForm?.PnrCost ?? 0) - (secForm?.PnrCost ?? 0),
                PnrContractCost = (firstForm?.PnrContractCost ?? 0) - (secForm?.PnrContractCost ?? 0),
                PnrNdsCost = (firstForm?.PnrNdsCost ?? 0) - (secForm?.PnrNdsCost ?? 0),
                SmrCost = (firstForm?.SmrCost ?? 0) - (secForm?.SmrCost ?? 0),
                SmrContractCost = (firstForm?.SmrContractCost ?? 0) - (secForm?.SmrContractCost ?? 0),
                SmrNdsCost = (firstForm?.SmrNdsCost ?? 0) - (secForm?.SmrNdsCost ?? 0),
                EquipmentCost = (firstForm?.EquipmentCost ?? 0) - (secForm?.EquipmentCost ?? 0),
                EquipmentContractCost = (firstForm?.EquipmentContractCost ?? 0) - (secForm?.EquipmentContractCost ?? 0),
                EquipmentNdsCost = (firstForm?.EquipmentNdsCost ?? 0) - (secForm?.EquipmentNdsCost ?? 0),
                EquipmentClientCost = (firstForm?.EquipmentClientCost ?? 0) - (secForm?.EquipmentClientCost ?? 0),
                OtherExpensesCost = (firstForm?.OtherExpensesCost ?? 0) - (secForm?.OtherExpensesCost ?? 0),
                OtherExpensesNdsCost = (firstForm?.OtherExpensesNdsCost ?? 0) - (secForm?.OtherExpensesNdsCost ?? 0),
                AdditionalCost = (firstForm?.AdditionalCost ?? 0) - (secForm?.AdditionalCost ?? 0),
                AdditionalContractCost = (firstForm?.AdditionalContractCost ?? 0) - (secForm?.AdditionalContractCost ?? 0),
                AdditionalNdsCost = (firstForm?.AdditionalNdsCost ?? 0) - (secForm?.AdditionalNdsCost ?? 0),
                GenServiceCost = (firstForm?.GenServiceCost ?? 0) - (secForm?.GenServiceCost ?? 0),
                MaterialCost = (firstForm?.MaterialCost ?? 0) - (secForm?.MaterialCost ?? 0),
                MaterialClientCost = (firstForm?.MaterialClientCost ?? 0) - (secForm?.MaterialClientCost ?? 0),
                Reserve = (firstForm?.Reserve ?? 0) - (secForm?.Reserve ?? 0),
                CostToConstructionIndustryFund = (firstForm?.CostToConstructionIndustryFund ?? 0) - (secForm?.CostToConstructionIndustryFund ?? 0),
                CostStatisticReportOfContractor = (firstForm?.CostStatisticReportOfContractor ?? 0) - (secForm?.CostStatisticReportOfContractor ?? 0),
                OffsetCurrentPrepayment = (firstForm?.OffsetCurrentPrepayment ?? 0) - (secForm?.OffsetCurrentPrepayment ?? 0),
                OffsetTargetPrepayment = (firstForm?.OffsetTargetPrepayment ?? 0) - (secForm?.OffsetTargetPrepayment ?? 0) 
            };
        }

        /// <summary>
        /// Умножает значения стоимостей первого объекта на величину переменной val
        /// </summary>
        /// <param name="firstForm"></param>
        /// <param name="secForm"></param>
        /// <returns>Новый объект равный первому, у котрого стоимости уноженны на величину val</returns>
        public static FormDTO operator * (int val, FormDTO? firstForm)
        {
            return new FormDTO
            {
                Id = firstForm.Id,
                Period = firstForm.Period,
                DateSigning = firstForm.DateSigning,
                IsExemptFromVAT = firstForm.IsExemptFromVAT,
                IsOwnForces = firstForm.IsOwnForces,
                ContractId = firstForm.ContractId,

                PnrCost = val * (firstForm?.PnrCost??0),
                PnrContractCost = val * (firstForm?.PnrContractCost ?? 0),
                PnrNdsCost = val * (firstForm?.PnrNdsCost ?? 0),
                SmrCost = val * (firstForm?.SmrCost ?? 0),
                SmrContractCost = val * (firstForm?.SmrContractCost ?? 0),
                SmrNdsCost = val * (firstForm?.SmrNdsCost ?? 0),
                EquipmentCost = val * (firstForm?.EquipmentCost ?? 0),
                EquipmentContractCost = val * (firstForm?.EquipmentContractCost ?? 0),
                EquipmentNdsCost = val * (firstForm?.EquipmentNdsCost ?? 0),
                EquipmentClientCost = val * (firstForm?.EquipmentClientCost ?? 0),
                OtherExpensesCost = val * (firstForm?.OtherExpensesCost ?? 0),
                OtherExpensesNdsCost = val * (firstForm?.OtherExpensesNdsCost ?? 0),
                AdditionalCost = val * (firstForm?.AdditionalCost ?? 0),
                AdditionalContractCost = val * (firstForm?.AdditionalContractCost ?? 0),
                AdditionalNdsCost = val * (firstForm?.AdditionalNdsCost ?? 0),
                GenServiceCost = val * (firstForm?.GenServiceCost ?? 0),
                MaterialCost = val * (firstForm?.MaterialCost ?? 0),
                MaterialClientCost = val * (firstForm?.MaterialClientCost ?? 0),
                Reserve = val * (firstForm?.Reserve ?? 0),
                CostToConstructionIndustryFund = val * (firstForm?.CostToConstructionIndustryFund ?? 0),
                CostStatisticReportOfContractor = val * (firstForm?.CostStatisticReportOfContractor ?? 0),
                OffsetCurrentPrepayment = val * (firstForm?.OffsetCurrentPrepayment ?? 0),
                OffsetTargetPrepayment = val * (firstForm?.OffsetTargetPrepayment ?? 0)
            };
        }
    }
}
