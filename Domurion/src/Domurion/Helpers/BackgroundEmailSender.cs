namespace Domurion.Helpers
{
    public class BackgroundEmailSender : BackgroundService
    {
        private readonly IEmailQueue _queue;
        private readonly MailKitEmailSender _sender;
        private readonly ILogger<BackgroundEmailSender> _logger;

        public BackgroundEmailSender(IEmailQueue queue, MailKitEmailSender sender, ILogger<BackgroundEmailSender> logger)
        {
            _queue = queue;
            _sender = sender;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var msg in _queue.DequeueAllAsync(stoppingToken))
            {
                int attempt = 0;
                while (attempt < 3)
                {
                    try
                    {
                        await _sender.SendAsync(msg, stoppingToken);
                        break;
                    }
                    catch (Exception ex)
                    {
                        attempt++;
                        _logger.LogWarning(ex, "Attempt {attempt} failed to send email to {to}", attempt, msg.To);
                        await Task.Delay(TimeSpan.FromSeconds(2 * attempt), stoppingToken);
                    }
                }
            }
        }
    }
}
