using Domurion.Data;
using Domurion.Models;
using Domurion.Helpers;

namespace Domurion.Services
{
    public class UserService(DataContext context) : IUserService
    {
        private readonly DataContext _context = context;

        public User Register(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Username and password are required.");

            if (!Helper.IsStrongPassword(password))
                throw new ArgumentException("Password does not meet strength requirements.");

            if (_context.Users.Any(u => u.Username == username))
                throw new InvalidOperationException("Username already exists.");

            var hashed = Helper.HashPassword(password);
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
            if (user != null && Helper.VerifyPassword(password, user.PasswordHash))
                return user;
            return null;
        }

        public User? GetByUsername(string username)
        {
            return _context.Users.FirstOrDefault(u => u.Username == username);
        }

        public User UpdateUser(Guid userId, string? newUsername, string? newPassword)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == userId)
                ?? throw new KeyNotFoundException("User not found.");

            if (newUsername != null)
            {
                if (_context.Users.Any(u => u.Username == newUsername && u.Id != userId))
                    throw new InvalidOperationException("Username already exists.");
                user.Username = newUsername;
            }
            if (newPassword != null)
            {
                if (!Helper.IsStrongPassword(newPassword))
                    throw new ArgumentException("Password does not meet strength requirements.");
                user.PasswordHash = Helper.HashPassword(newPassword);
            }
            _context.SaveChanges();
            return user;
        }
        public void DeleteUser(Guid userId)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == userId)
                ?? throw new KeyNotFoundException("User not found.");
            // Remove all credentials for this user
            var credentials = _context.Credentials.Where(c => c.UserId == userId).ToList();
            _context.Credentials.RemoveRange(credentials);
            _context.Users.Remove(user);
            _context.SaveChanges();
        }
    }
}