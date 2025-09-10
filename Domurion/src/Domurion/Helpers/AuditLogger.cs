using Domurion.Models;

namespace Domurion.Helpers
{
    public static class AuditLogger
    {
        public static void Log(Data.DataContext context, Guid userId, string username, Guid? credentialId, string action, string? ipAddress, string? site = null)
        {
            var log = new AuditLog
            {
                UserId = userId,
                Username = username,
                CredentialId = credentialId,
                Action = action,
                Timestamp = DateTime.UtcNow,
                IpAddress = ipAddress ?? string.Empty,
                Site = site
            };
            context.AuditLogs.Add(log);
            context.SaveChanges();
        }
    }
}
