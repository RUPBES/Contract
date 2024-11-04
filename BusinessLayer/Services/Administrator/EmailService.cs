using BusinessLayer.Interfaces.CommonInterfaces;
using BusinessLayer.Models.Settings;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

namespace BusinessLayer.Services.Administrator
{
    public class EmailService : IEmailService
    {
        private readonly EmailOptions _emailOptions;


        public EmailService(IOptions<EmailOptions> emailOptions)
        {
            _emailOptions = emailOptions.Value;           
        }

        public async Task SendAsync(string email, string subject, string message, Attachment attachment)
        {
            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress(/*titleSender*/_emailOptions.NameFrom, _emailOptions.From));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Subject = subject;
           
            BodyBuilder builder = new BodyBuilder();

            builder.HtmlBody = message;

            if (attachment is not null)
            {               
                builder.Attachments.Add(attachment.FileName, attachment.Bytes);
            }
            
            emailMessage.Body = builder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(_emailOptions.Host, _emailOptions.Port);
                await client.AuthenticateAsync(_emailOptions.UserName, _emailOptions.UserLogin);
                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);
            }
        }
    }
}
