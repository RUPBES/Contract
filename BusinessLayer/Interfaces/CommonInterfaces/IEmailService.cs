using BusinessLayer.Models.Settings;

namespace BusinessLayer.Interfaces.CommonInterfaces
{
    public interface IEmailService
    {
        Task SendAsync(string email, string subject, string message, Attachment attachment = null);
    }
}
