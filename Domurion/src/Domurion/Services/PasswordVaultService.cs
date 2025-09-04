using Domurion.Models;
using System.Security.Cryptography;
using System.Text;
using Domurion.Data;
using Domurion.Services.Interfaces;

namespace Domurion.Services
{
    public class PasswordVaultService(DataContext context) : IPasswordVaultService
    {
        private readonly DataContext _context = context;

        public Credential AddCredential(Guid userId, string site, string username, string password)
        {
            if (string.IsNullOrWhiteSpace(site) || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Site, username, and password are required.");

            var encryptedPassword = EncryptPassword(password);
            var credential = new Credential
            {
                UserId = userId,
                Site = site,
                Username = username,
                EncryptedPassword = encryptedPassword
            };
            _context.Credentials.Add(credential);
            _context.SaveChanges();
            return credential;
        }

        public IEnumerable<Credential> GetCredentials(Guid userId)
        {
            return [.. _context.Credentials.Where(c => c.UserId == userId)];
        }

        public string RetrievePassword(Guid credentialId, Guid userId)
        {
            var credential = _context.Credentials
                .FirstOrDefault(c => c.Id == credentialId && c.UserId == userId)
                ?? throw new KeyNotFoundException("Credential not found.");
            return DecryptPassword(credential.EncryptedPassword);
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