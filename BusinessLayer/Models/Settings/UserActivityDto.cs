using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Models.Settings
{
    /// <summary>
    /// Активность одного пользователя за период
    /// </summary>

    public class UserActivityDto
    {
        public string? NameIdentifier { get; set; }
        public string? UniqueName { get; set; }
        public string FullName { get; set; } = string.Empty;
        public DateTime CreationTime { get; set; }

        // CRUD-операции
        public int Creates { get; set; }
        public int Reads { get; set; }
        public int Updates { get; set; }
        public int Deletes { get; set; }

        // Итого
        public int Total => Creates + Reads + Updates + Deletes;

        // Email для связи с токеном
        public string Email { get; set; } = string.Empty;

        // Последняя активность
        public DateTime? LastActivity { get; set; }
    }

    /// <summary>
    /// Точка линейного графика: метка + количество операций
    /// </summary>
    public class ActivityTimelinePoint
    {
        public string Label { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    /// <summary>
    /// Суммарная карточка-метрика
    /// </summary>
    public class DashboardMetric
    {
        public string Title { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public string Color { get; set; } = "primary";
    }

    /// <summary>
    /// Главная ViewModel дашборда
    /// </summary>
    public class ActivityDashboardViewModel
    {
        public string Period { get; set; } = "month";

        public List<DashboardMetric> Metrics { get; set; } = new();
        public List<UserActivityDto> Users { get; set; } = new();

        // Для линейного графика: ключ = "day"|"month"|"year"
        public Dictionary<string, List<ActivityTimelinePoint>> Timeline { get; set; } = new();

        // Суммы для круговой диаграммы
        public int TotalCreates { get; set; }
        public int TotalReads { get; set; }
        public int TotalUpdates { get; set; }
        public int TotalDeletes { get; set; }

        public int GrandTotal => TotalCreates + TotalReads + TotalUpdates + TotalDeletes;
    }
}
