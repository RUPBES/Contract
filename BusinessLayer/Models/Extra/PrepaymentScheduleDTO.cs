using BusinessLayer.Models.KDO;

namespace BusinessLayer.Models.Extra
{
    public class PrepaymentScheduleDTO
    {
        /// <summary>сводка по целевому авансу </summary>
        public AdvanceSummaryDTO TargetSummary { get; set; } = null!;   // сводка по целевому авансу

        /// <summary> сводка по текущему авансу</summary>
        public AdvanceSummaryDTO CurrentSummary { get; set; } = null!;  // сводка по текущему авансу

        /// <summary>периодов плана (по версиям) и фактам.</summary>
        public List<PeriodAdvanceDTO> Periods { get; set; } = new();
    }
   
    /// <summary>Сводные показатели (используется и для целевого, и для текущего аванса).</summary>
    public class AdvanceSummaryDTO
    {
        /// <summary>Всего аванса </summary>
        public decimal Total { get; set; }
        /// <summary>Всего полученного аванса </summary>
        public decimal Received { get; set; } // полученные авансы
        /// <summary>Всего отработанного аванса (справка с3-а)</summary>
        public decimal Settled { get; set; } // отработанные авансы (форма с3-а)
        /// <summary>Всего погашено аванса </summary>
        public decimal Unsettled => Received - Settled;
    }

    /// <summary>Данные по одному периоду (месяцу) договора.</summary>
    public class PeriodAdvanceDTO
    {
        public DateTime Period { get; set; }

        public int PrepId { get; set; }
        /// <summary>Плановые значения по каждой версии (Agreements), в т.ч. исходный договор.</summary>
        public List<PeriodVersionDTO> Versions { get; set; } = new();

        /// <summary>Получено текущих — один на период (не привязан к версии ДС).</summary>
        public decimal? PrepFactCurrent { get; set; }
        /// <summary>Получено тецелевых — один на период (не привязан к версии ДС).</summary>
        public decimal? PrepFactTarget { get; set; }

        /// <summary>Факт текущие зачет — один на период (не привязан к версии ДС).</summary>
        public decimal? FactCurrent { get; set; }

        /// <summary>Факт целевые зачет— один на период (не привязан к версии ДС).</summary>
        public decimal? FactTarget { get; set; }
        /// <summary>Файлы справок с3-а.</summary>
        public List<FileDTO> Files { get; set; } = new();
    }

    /// <summary>Плановое значение периода в рамках конкретной версии (AgreementId).</summary>
    public class PeriodVersionDTO
    {
        public int AmendmentId { get; set; }   
        public string? AmendNumber { get; set; }      // null = "Договор (исходно)", иначе "ДС №1", "ДС №2"...
        public decimal CurrentAmount { get; set; }
        public decimal TargetAmount { get; set; }
    }
}
