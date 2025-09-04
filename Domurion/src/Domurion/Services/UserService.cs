using Domurion.Data;
using Domurion.Models;
using System.Security.Cryptography;
using System.Text;

namespace Domurion.Services
{
    public class UserService : IUserService
    {
        private readonly DataContext _context;
        public UserService(DataContext context)
        {
            _context = context;
        }

        public User Register(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Username and password are required.");

            if (_context.Users.Any(u => u.Username == username))
                throw new InvalidOperationException("Username already exists.");

            var hashed = HashPassword(password);
            var user = new User { Username = username, PasswordHash = hashed };
            _context.Users.Add(user);
            _context.SaveChanges();
            return user;
        }

        public User? Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Username and password are required.");

            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user != null && VerifyPassword(password, user.PasswordHash))
                return user;
            return null;
        }

        private static string HashPassword(string password)
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

        private static bool VerifyPassword(string password, string storedHash)
        {
            var parts = storedHash.Split(':');
            if (parts.Length != 2) return false;
            byte[] salt = Convert.FromBase64String(parts[0]);
            byte[] hash = Convert.FromBase64String(parts[1]);
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256);
            byte[] testHash = pbkdf2.GetBytes(32);
            return CryptographicOperations.FixedTimeEquals(hash, testHash);
        }
    }
}