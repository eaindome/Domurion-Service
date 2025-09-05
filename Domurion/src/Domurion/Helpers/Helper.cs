using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Text;

namespace Domurion.Helpers
{
    public static class Helper
    {
        public static bool IsStrongPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
                return false;

            // At least one uppercase, one lowercase, one digit, one special character
            var pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[^a-zA-Z\\d]).{8,}$";
            return Regex.IsMatch(password, pattern);
        }

        public static string HashPassword(string password)
        {
            // Use PBKDF2 with a random salt
            using var rng = RandomNumberGenerator.Create();
            byte[] salt = new byte[16];
            rng.GetBytes(salt);
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(32);
            // Store salt and hash together (salt:hash)
            return Convert.ToBase64String(salt) + ":" + Convert.ToBase64String(hash);
        }

        public static bool VerifyPassword(string password, string storedHash)
        {
            var parts = storedHash.Split(':');
            if (parts.Length != 2) return false;
            byte[] salt = Convert.FromBase64String(parts[0]);
            byte[] hash = Convert.FromBase64String(parts[1]);
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256);
            byte[] testHash = pbkdf2.GetBytes(32);
            return CryptographicOperations.FixedTimeEquals(hash, testHash);
        }

        public static string ComputeHmac(string data, string key)
        {
            using var hmac = new HMACSHA256(Convert.FromBase64String(key));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return Convert.ToBase64String(hash);
        }
    }
}