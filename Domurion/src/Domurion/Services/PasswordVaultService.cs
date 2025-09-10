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

            var encryptedPassword = EncryptPassword(password);
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
            Domurion.Helpers.AuditLogger.Log(_context, userId, user?.Username ?? string.Empty, credential.Id, "AddCredential", ipAddress, site);
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
            Domurion.Helpers.AuditLogger.Log(_context, userId, user?.Username ?? string.Empty, credentialId, "RetrievePassword", ipAddress, credential.Site);
            return DecryptPassword(credential.EncryptedPassword);
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
                var encryptedPassword = EncryptPassword(password);
                credential.EncryptedPassword = encryptedPassword;
                var hmacKey = Environment.GetEnvironmentVariable("HMAC_KEY");
                if (string.IsNullOrWhiteSpace(hmacKey))
                    throw new InvalidOperationException("HMAC_KEY environment variable is not set.");
                credential.IntegrityHash = Helper.ComputeHmac(encryptedPassword, hmacKey);
            }

            _context.SaveChanges();
            // Audit log
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            Domurion.Helpers.AuditLogger.Log(_context, userId, user?.Username ?? string.Empty, credentialId, "UpdateCredential", ipAddress, credential.Site);
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
            Domurion.Helpers.AuditLogger.Log(_context, userId, user?.Username ?? string.Empty, credentialId, "DeleteCredential", ipAddress, credential.Site);
        }

        private static string EncryptPassword(string password)
        {
            // AES encryption for production use. Store key/IV securely!
            using var aes = Aes.Create();
            aes.Key = GetAesKey();
            aes.IV = GetAesIV();
            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            var plainBytes = Encoding.UTF8.GetBytes(password);
            var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
            return Convert.ToBase64String(encryptedBytes);
        }

        private static string DecryptPassword(string encryptedPassword)
        {
            // AES decryption
            using var aes = Aes.Create();
            aes.Key = GetAesKey();
            aes.IV = GetAesIV();
            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            var encryptedBytes = Convert.FromBase64String(encryptedPassword);
            var decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
            return Encoding.UTF8.GetString(decryptedBytes);
        }

        private static byte[] GetAesKey()
        {
            var keyBase64 = Environment.GetEnvironmentVariable("AES_KEY");
            if (string.IsNullOrWhiteSpace(keyBase64))
                throw new InvalidOperationException("AES_KEY environment variable is not set.");
            var key = Convert.FromBase64String(keyBase64);
            if (key.Length != 32)
                throw new InvalidOperationException("AES_KEY must be 32 bytes (Base64-encoded).");
            return key;
        }

        private static byte[] GetAesIV()
        {
            var ivBase64 = Environment.GetEnvironmentVariable("AES_IV");
            if (string.IsNullOrWhiteSpace(ivBase64))
                throw new InvalidOperationException("AES_IV environment variable is not set.");
            var iv = Convert.FromBase64String(ivBase64);
            if (iv.Length != 16)
                throw new InvalidOperationException("AES_IV must be 16 bytes (Base64-encoded).");
            return iv;
        }
    }
}