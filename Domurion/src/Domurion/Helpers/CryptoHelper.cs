using System.Text;
using System.Security.Cryptography;

namespace Domurion.Helpers
{
    public static class CryptoHelper
    {
        public static string EncryptPassword(string password)
        {
            using var aes = Aes.Create();
            aes.Key = GetAesKey();
            aes.IV = GetAesIV();
            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            var plainBytes = Encoding.UTF8.GetBytes(password);
            var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
            return Convert.ToBase64String(encryptedBytes);
        }

        public static string DecryptPassword(string encryptedPassword)
        {
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
