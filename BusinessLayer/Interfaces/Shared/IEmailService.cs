using BusinessLayer.Models.Settings;

namespace BusinessLayer.Interfaces.Shared
{
    public interface IEmailService
    {
        Task SendAsync(string email, string subject, string message, Attachment attachment = null);
    }
}
