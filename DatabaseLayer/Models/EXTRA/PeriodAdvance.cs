namespace DatabaseLayer.Models.EXTRA
{

    public class PrepaymentSchedule
    {
        public AdvanceSummary TargetSummary { get; set; } = null!;   // сводка по целевому авансу
        public AdvanceSummary CurrentSummary { get; set; } = null!;  // сводка по текущему авансу

        /// <summary>15 периодов с планом (по версиям) и фактом.</summary>
        public List<PeriodAdvance> Periods { get; set; } = new();
    }

    public class AdvanceSummary
    {
        public decimal Total { get; set; }
        public decimal Received { get; set; } // полученные авансы
        public decimal Settled { get; set; } // отработанные авансы (форма с3-а)
        public decimal Unsettled => Received - Settled;
    }

    // ==========================
    // Промежуточная плоская модель для Dapper
    // ==========================

    public class PeriodAdvance
    {
        public DateTime Period { get; set; }
        public int PrepId { get; set; }
        /// <summary>Плановые значения по каждой версии (Agreements), в т.ч. исходный договор.</summary>
        public List<PeriodVersion> Versions { get; set; } = new();

        /// <summary>Получено — один на период (не привязан к версии ДС).</summary>
        public decimal? PrepFactCurrent { get; set; }
        public decimal? PrepFactTarget { get; set; }

        /// <summary>Факт — один на период (не привязан к версии ДС).</summary>
        public decimal? FactCurrent { get; set; }
        public decimal? FactTarget { get; set; }
        public List<DatabaseLayer.Models.KDO.File> Files { get; set; } = new();
    }

    /// <summary>Плановое значение периода в рамках конкретной версии (AgreementId).</summary>
    public class PeriodVersion
    {
        public int AmendmentId { get; set; }
        public string? AmendNumber { get; set; }      // null = "Договор (исходно)", иначе "ДС №1", "ДС №2"...
        public decimal CurrentAmount { get; set; }
        public decimal TargetAmount { get; set; }
    }        
}
