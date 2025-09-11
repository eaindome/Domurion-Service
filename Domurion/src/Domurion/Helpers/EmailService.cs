using System.Net;
using System.Net.Mail;

namespace Domurion.Helpers
{
    public class EmailService(IConfiguration config)
    {
        private readonly IConfiguration _config = config;

        public void SendEmail(string to, string subject, string body)
        {
            var smtpHost = _config["Smtp:Host"];
            var smtpPort = int.Parse(_config["Smtp:Port"] ?? "587");
            var smtpUser = _config["Smtp:Username"];
            var smtpPass = _config["Smtp:Password"];
            var from = _config["Smtp:From"] ?? smtpUser;
            if (string.IsNullOrWhiteSpace(from))
            {
                throw new InvalidOperationException("Sender email address ('Smtp:From' or 'Smtp:Username') is not configured.");
            }

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                EnableSsl = true
            };
            var mail = new MailMessage(from, to, subject, body) { IsBodyHtml = false };
            client.Send(mail);
        }
    }
}
