using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace BusinessLayer.Helpers
{
    public static class NotificationHelper
    {
        public static void SetNotification(ITempDataDictionary tempData, string message, NotificationType type = NotificationType.Info)
        {
            tempData["NotificationMessage"] = message;
            tempData["NotificationType"] = type.ToString();
        }

        public static (string Message, string Type) GetNotification(ITempDataDictionary tempData)
        {
            var message = tempData["NotificationMessage"]?.ToString();
            var type = tempData["NotificationType"]?.ToString() ?? NotificationType.Info.ToString();
            
            return (message, type);
        }
    }

    public enum NotificationType
    {
        Success,
        Error,
        Warning,
        Info
    }
} 