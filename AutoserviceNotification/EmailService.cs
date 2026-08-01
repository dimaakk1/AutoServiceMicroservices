using System.Net;
using System.Net.Mail;

namespace AutoserviceNotification
{
    public class EmailService : IEmailService
    {
        private readonly string _email;
        private readonly string _password;
        private readonly string _smtpServer;
        private readonly int _smtpPort;

        public EmailService(IConfiguration configuration)
        {
            _email = configuration["Email:Address"]
                ?? Environment.GetEnvironmentVariable("EMAIL")
                ?? throw new InvalidOperationException("Email address is not configured.");
            _password = configuration["Email:Password"]
                ?? Environment.GetEnvironmentVariable("EMAIL_PASSWORD")
                ?? throw new InvalidOperationException("Email password is not configured.");
            _smtpServer = configuration["Email:SmtpServer"] ?? "smtp.gmail.com";
            _smtpPort = configuration.GetValue<int?>("Email:SmtpPort") ?? 587;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            using var client = new SmtpClient(_smtpServer, _smtpPort)
            {
                Credentials = new NetworkCredential(_email, _password),
                EnableSsl = true
            };

            using var mail = new MailMessage(_email, to)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            await client.SendMailAsync(mail);
        }
    }
}
