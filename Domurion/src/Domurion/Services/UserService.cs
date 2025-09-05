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
    }
}