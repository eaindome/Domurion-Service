using Domurion.Models;

namespace Domurion.Helpers
{
    public static class AuditLogger
    {
        public static void Log(Data.DataContext context, Guid userId, string username, Guid? credentialId, string action, string? ipAddress, string? site = null)
        {
            // Mask recovery codes if present in the 'site' parameter
            string maskedSite = site ?? string.Empty;
            if (!string.IsNullOrEmpty(site) && (action.Contains("RecoveryCode", System.StringComparison.OrdinalIgnoreCase) || action.Contains("2FA", System.StringComparison.OrdinalIgnoreCase)))
            {
                maskedSite = "[MASKED]";
            }
            var log = new AuditLog
            {
                UserId = userId,
                Username = username,
                CredentialId = credentialId,
                Action = action,
                Timestamp = DateTime.UtcNow,
                IpAddress = ipAddress ?? string.Empty,
                Site = maskedSite
            };
            context.AuditLogs.Add(log);
            context.SaveChanges();
        }
    }
}
