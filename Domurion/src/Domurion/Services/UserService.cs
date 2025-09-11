using Domurion.Data;
using Domurion.Models;
using Domurion.Helpers;
using Domurion.Services.Interfaces;

namespace Domurion.Services
{
    public class UserService(DataContext context) : IUserService
    {
        private readonly DataContext _context = context;

        public User Register(string username, string password, string? name = null)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Username and password are required.");

            if (!Helper.IsStrongPassword(password))
                throw new ArgumentException("Password does not meet strength requirements.");

            if (_context.Users.Any(u => u.Username == username))
                throw new InvalidOperationException("Username already exists.");

            var hashed = Helper.HashPassword(password);
            var user = new User { Username = username, PasswordHash = hashed, Name = name };
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

        public User UpdateUser(Guid userId, string? newUsername, string? newPassword, string? newName = null)
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

                // Check password history (last 5)
                var newHash = Helper.HashPassword(newPassword);
                var history = _context.PasswordHistories
                    .Where(h => h.UserId == userId)
                    .OrderByDescending(h => h.ChangedAt)
                    .Take(5)
                    .Select(h => h.PasswordHash)
                    .ToList();
                // Also check current password
                history.Add(user.PasswordHash);
                foreach (var oldHash in history)
                {
                    if (Helper.VerifyPassword(newPassword, oldHash))
                        throw new ArgumentException("You cannot reuse your previous 5 passwords.");
                }

                // Store old password in history
                _context.PasswordHistories.Add(new PasswordHistory
                {
                    UserId = userId,
                    PasswordHash = user.PasswordHash,
                    ChangedAt = DateTime.UtcNow
                });

                // Keep only last 5 history entries
                var allHistory = _context.PasswordHistories
                    .Where(h => h.UserId == userId)
                    .OrderByDescending(h => h.ChangedAt)
                    .ToList();
                if (allHistory.Count > 5)
                {
                    var toRemove = allHistory.Skip(5).ToList();
                    _context.PasswordHistories.RemoveRange(toRemove);
                }

                user.PasswordHash = newHash;
            }
            if (newName != null)
            {
                user.Name = newName;
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

        public User CreateExternalUser(string email, string? name, string provider)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email is required for external users.");
            var existingUser = _context.Users.FirstOrDefault(u => u.Username == email);
            if (existingUser != null)
            {
                if (string.IsNullOrEmpty(existingUser.AuthProvider))
                    throw new InvalidOperationException("A local account with this email already exists. Please log in with your password or link your Google account in settings.");
                return existingUser;
            }
            var user = new User
            {
                Username = email,
                PasswordHash = string.Empty, // No password for external users
                AuthProvider = provider
            };
            _context.Users.Add(user);
            _context.SaveChanges();
            return user;
        }

        // Link a Google account to an existing user
        public bool LinkGoogleAccount(Guid userId, string googleId)
        {
            if (string.IsNullOrWhiteSpace(googleId))
                return false;
            // Ensure no other user has this GoogleId
            if (_context.Users.Any(u => u.GoogleId == googleId && u.Id != userId))
                return false;
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
                return false;
            user.GoogleId = googleId;
            user.AuthProvider = "Google";
            _context.SaveChanges();
            // Audit log
            AuditLogger.Log(_context, user.Id, user.Username, null, "LinkGoogleAccount", null);
            return true;
        }
        
        // Unlink a Google account from an existing user
        public bool UnlinkGoogleAccount(Guid userId)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null || string.IsNullOrEmpty(user.GoogleId))
                return false;
            user.GoogleId = null;
            user.AuthProvider = null;
            _context.SaveChanges();
            // Audit log
            AuditLogger.Log(_context, user.Id, user.Username, null, "UnlinkGoogleAccount", null);
            return true;
        }
    }
}