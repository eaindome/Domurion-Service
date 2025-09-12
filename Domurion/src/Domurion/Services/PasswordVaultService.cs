using Domurion.Models;
using Domurion.Helpers;
using Domurion.Data;
using Domurion.Services.Interfaces;

namespace Domurion.Services
{
    public class PasswordVaultService(DataContext context) : IPasswordVaultService
    {
        private readonly DataContext _context = context;

        public Credential AddCredential(Guid userId, string site, string username, string password, string? ipAddress = null)
        {
            if (string.IsNullOrWhiteSpace(site) || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Site, username, and password are required.");

            if (!Helper.IsStrongPassword(password))
                throw new ArgumentException("Password does not meet strength requirements.");

            var encryptedPassword = CryptoHelper.EncryptPassword(password);
            var hmacKey = Environment.GetEnvironmentVariable("HMAC_KEY");
            if (string.IsNullOrWhiteSpace(hmacKey))
                throw new InvalidOperationException("HMAC_KEY environment variable is not set.");
            var integrityHash = Helper.ComputeHmac(encryptedPassword, hmacKey);
            var credential = new Credential
            {
                UserId = userId,
                Site = site,
                Username = username,
                EncryptedPassword = encryptedPassword,
                IntegrityHash = integrityHash
            };
            _context.Credentials.Add(credential);
            _context.SaveChanges();
            // Audit log
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            AuditLogger.Log(_context, userId, user?.Username ?? string.Empty, credential.Id, "AddCredential", ipAddress, site);
            return credential;
        }

        public IEnumerable<Credential> GetCredentials(Guid userId)
        {
            return [.. _context.Credentials.Where(c => c.UserId == userId)];
        }

        public string RetrievePassword(Guid credentialId, Guid userId, string? ipAddress = null)
        {
            var hmacKey = Environment.GetEnvironmentVariable("HMAC_KEY");
            if (string.IsNullOrWhiteSpace(hmacKey))
                throw new InvalidOperationException("HMAC_KEY environment variable is not set.");
            var expectedHash = Helper.ComputeHmac(
                _context.Credentials
                    .Where(c => c.Id == credentialId && c.UserId == userId)
                    .Select(c => c.EncryptedPassword)
                    .FirstOrDefault() ?? string.Empty,
                hmacKey);
            var credential = _context.Credentials
                .FirstOrDefault(c => c.Id == credentialId && c.UserId == userId)
                ?? throw new KeyNotFoundException("Credential not found.");
            if (credential.IntegrityHash != expectedHash)
                throw new InvalidOperationException("Data integrity check failed.");
            // Audit log
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            AuditLogger.Log(_context, userId, user?.Username ?? string.Empty, credentialId, "RetrievePassword", ipAddress, credential.Site);
            return CryptoHelper.DecryptPassword(credential.EncryptedPassword);
        }

        public Credential UpdateCredential(Guid credentialId, Guid userId, string? site, string? username, string? password, string? ipAddress = null)
        {
            var credential = _context.Credentials.FirstOrDefault(c => c.Id == credentialId && c.UserId == userId)
                ?? throw new KeyNotFoundException("Credential not found.");

            if (site != null)
                credential.Site = site;
            if (username != null)
                credential.Username = username;
            if (password != null)
            {
                if (!Helper.IsStrongPassword(password))
                    throw new ArgumentException("Password does not meet strength requirements.");
                var encryptedPassword = CryptoHelper.EncryptPassword(password);
                credential.EncryptedPassword = encryptedPassword;
                var hmacKey = Environment.GetEnvironmentVariable("HMAC_KEY");
                if (string.IsNullOrWhiteSpace(hmacKey))
                    throw new InvalidOperationException("HMAC_KEY environment variable is not set.");
                credential.IntegrityHash = Helper.ComputeHmac(encryptedPassword, hmacKey);
            }

            _context.SaveChanges();
            // Audit log
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            AuditLogger.Log(_context, userId, user?.Username ?? string.Empty, credentialId, "UpdateCredential", ipAddress, credential.Site);
            return credential;
        }

        public void DeleteCredential(Guid credentialId, Guid userId, string? ipAddress = null)
        {
            var credential = _context.Credentials.FirstOrDefault(c => c.Id == credentialId && c.UserId == userId)
                ?? throw new KeyNotFoundException("Credential not found.");
            _context.Credentials.Remove(credential);
            _context.SaveChanges();
            // Audit log
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            AuditLogger.Log(_context, userId, user?.Username ?? string.Empty, credentialId, "DeleteCredential", ipAddress, credential.Site);
        }

        // Share a credential with another user by username
        public Credential ShareCredential(Guid credentialId, Guid fromUserId, string toUsername, string? ipAddress = null)
        {
            var fromUser = _context.Users.FirstOrDefault(u => u.Id == fromUserId)
                ?? throw new KeyNotFoundException("Sender user not found.");
            var toUser = _context.Users.FirstOrDefault(u => u.Username == toUsername)
                ?? throw new KeyNotFoundException("Recipient user not found.");
            if (fromUser.Id == toUser.Id)
                throw new ArgumentException("Cannot share credential with yourself.");

            var credential = _context.Credentials.FirstOrDefault(c => c.Id == credentialId && c.UserId == fromUserId)
                ?? throw new KeyNotFoundException("Credential not found.");

            // Duplicate the credential for the recipient
            var newCredential = new Credential
            {
                UserId = toUser.Id,
                Site = credential.Site,
                Username = credential.Username,
                EncryptedPassword = credential.EncryptedPassword,
                IntegrityHash = credential.IntegrityHash
            };
            _context.Credentials.Add(newCredential);
            _context.SaveChanges();

            // Audit log for both users
            AuditLogger.Log(_context, fromUser.Id, fromUser.Username, credential.Id, "ShareCredential", ipAddress, credential.Site);
            AuditLogger.Log(_context, toUser.Id, toUser.Username, newCredential.Id, "ReceiveSharedCredential", ipAddress, credential.Site);

            return newCredential;
        }
    }
}