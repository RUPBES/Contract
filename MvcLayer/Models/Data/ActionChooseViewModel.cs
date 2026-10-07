using System.Collections.Generic;

namespace MvcLayer.Models.Data
{
    /// <summary>
    /// Универсальная модель модального окна "выбор действия" (MD3).
    /// Используется для авансов, объёмов работ и любых других сущностей
    /// с одинаковым сценарием: либо выбрать доп.соглашение (Selector),
    /// либо показать статус + список действий (Alerts + Actions).
    /// </summary>
    public class ActionChooseViewModel
    {
        /// <summary>Технический заголовок (ViewData["Title"], breadcrumbs и т.п.)</summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>Куда ведёт крестик закрытия. Если null/пусто — крестик не рендерится.</summary>
        public string? CloseUrl { get; set; }

        /// <summary>Иконка верхнего смыслового блока: check_circle | warning | info | error</summary>
        public string HeadlineIcon { get; set; } = "info";

        /// <summary>Тип верхнего блока — определяет цвет tonal-круга и иконки.</summary>
        public AlertType HeadlineType { get; set; } = AlertType.Info;

        /// <summary>Заголовок смыслового блока, напр. "Аванс заполнен".</summary>
        public string HeadlineText { get; set; } = string.Empty;

        /// <summary>Поддерживающий текст под заголовком (необязательный).</summary>
        public string? SupportingText { get; set; }

        /// <summary>true — показываем список-picker (Selector), false — Alerts + Actions.</summary>
        public bool ShowSelector { get; set; }

        /// <summary>Блок выбора доп.соглашения (или иной сущности) — заполняется при ShowSelector = true.</summary>
        public SelectorBlock? Selector { get; set; }

        /// <summary>Дополнительные компактные строки-статусы — заполняется при ShowSelector = false.</summary>
        public List<AlertItem> Alerts { get; set; } = new();

        /// <summary>Кнопки действий внизу диалога — заполняется при ShowSelector = false.</summary>
        public List<ActionItem> Actions { get; set; } = new();
    }

    public class SelectorBlock
    {
        public string SelectName { get; set; } = "Id";
        public string FormController { get; set; } = string.Empty;
        public string FormAction { get; set; } = "Create";
        public List<SelectOptionItem> Options { get; set; } = new();

        /// <summary>Скрытые поля формы (аналог asp-for=... hidden в исходной вьюхе).</summary>
        public Dictionary<string, string?> HiddenFields { get; set; } = new();
    }

    public class SelectOptionItem
    {
        public string Value { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }

    public class AlertItem
    {
        public string Text { get; set; } = string.Empty;
        public AlertType Type { get; set; } = AlertType.Info;
    }

    public enum AlertType
    {
        Info,
        Success,
        Warning,
        Error
    }

    public class ActionItem
    {
        public string Text { get; set; } = string.Empty;

        /// <summary>Ключ иконки: add | search | check_circle | warning | info | error</summary>
        public string Icon { get; set; } = "add";

        public ActionVariant Variant { get; set; } = ActionVariant.Filled;

        /// <summary>Цветовой акцент filled/tonal-кнопки: primary | secondary | tertiary | create | edit</summary>
        public string Accent { get; set; } = "primary";

        public string Controller { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;

        /// <summary>Совместимо с asp-all-route-data.</summary>
        public Dictionary<string, string> RouteValues { get; set; } = new();
    }

    public enum ActionVariant
    {
        Filled,
        Tonal,
        Outlined,
        Text
    }
}