
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

        public static string GeneratePassword(int length = 16)
        {
            if (length < 8) length = 8;
            const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string lower = "abcdefghijklmnopqrstuvwxyz";
            const string digits = "0123456789";
            const string specials = "!@#$%^&*()-_=+[]{}|;:,.<>?";
            string all = upper + lower + digits + specials;
            var rnd = RandomNumberGenerator.Create();
            var chars = new char[length];
            // Ensure at least one of each type
            chars[0] = upper[GetRandomIndex(rnd, upper.Length)];
            chars[1] = lower[GetRandomIndex(rnd, lower.Length)];
            chars[2] = digits[GetRandomIndex(rnd, digits.Length)];
            chars[3] = specials[GetRandomIndex(rnd, specials.Length)];
            for (int i = 4; i < length; i++)
                chars[i] = all[GetRandomIndex(rnd, all.Length)];
            // Shuffle
            return new string(chars.OrderBy(_ => GetRandomIndex(rnd, length)).ToArray());
        }

        private static int GetRandomIndex(RandomNumberGenerator rng, int max)
        {
            var bytes = new byte[4];
            rng.GetBytes(bytes);
            return Math.Abs(BitConverter.ToInt32(bytes, 0)) % max;
        }
    }
}