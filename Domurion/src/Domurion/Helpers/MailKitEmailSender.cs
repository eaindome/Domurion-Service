using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Domurion.Helpers
{
    public class MailKitEmailSender
    {
        private readonly ILogger<MailKitEmailSender> _logger;
        private readonly IConfiguration _config;

        public MailKitEmailSender(IConfiguration config, ILogger<MailKitEmailSender> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
        {
            var host = Environment.GetEnvironmentVariable("SMTP_Host") ?? _config["Smtp:Host"] ?? "smtp.gmail.com";
            var port = int.TryParse(Environment.GetEnvironmentVariable("SMTP_Port") ?? _config["Smtp:Port"], out var p) ? p : 587;
            var user = Environment.GetEnvironmentVariable("SMTP_Username") ?? _config["Smtp:Username"];
            var pass = Environment.GetEnvironmentVariable("SMTP_Password") ?? _config["Smtp:Password"];
            var from = Environment.GetEnvironmentVariable("SMTP_From") ?? _config["Smtp:From"] ?? user;

            var mime = new MimeMessage();
            mime.From.Add(MailboxAddress.Parse(from));
            mime.To.Add(MailboxAddress.Parse(message.To));
            mime.Subject = message.Subject;
            mime.Body = new TextPart(message.IsHtml ? "html" : "plain") { Text = message.Body };

            using var client = new SmtpClient();
            try
            {
                _logger.LogInformation("Connecting to SMTP {host}:{port}", host, port);
                await client.ConnectAsync(host, port, SecureSocketOptions.StartTlsWhenAvailable, cancellationToken);
                await client.AuthenticateAsync(user, pass, cancellationToken);
                await client.SendAsync(mime, cancellationToken);
                await client.DisconnectAsync(true, cancellationToken);
                _logger.LogInformation("Email sent to {to}", message.To);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {to}", message.To);
                throw;
            }
        }
    }
}
