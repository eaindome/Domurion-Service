using System.Net;
using System.Net.Mail;

namespace Domurion.Helpers
{
    public class EmailService(IConfiguration config)
    {
        private readonly IConfiguration _config = config;

        public void SendEmail(string to, string subject, string body, bool isHtml = false)
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
            var mail = new MailMessage(from, to, subject, body) { IsBodyHtml = isHtml };
            client.Send(mail);
        }

        public string RenderTemplate(string templatePath, Dictionary<string, string> placeholders)
        {
            // Try to resolve absolute path if needed
            string fullPath = templatePath;
            if (!Path.IsPathRooted(templatePath))
            {
                fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, templatePath.Replace('/', Path.DirectorySeparatorChar));
            }
            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"Email template not found: {fullPath}");
            }
            var html = File.ReadAllText(fullPath);
            if (placeholders != null)
            {
                foreach (var kvp in placeholders)
                {
                    html = html.Replace($"{{{{{kvp.Key}}}}}", kvp.Value ?? "");
                }
            }
            return html;
        }
    }
}
