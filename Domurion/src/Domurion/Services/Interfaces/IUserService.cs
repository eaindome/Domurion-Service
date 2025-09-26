using Domurion.Models;

namespace Domurion.Services.Interfaces
{
    public interface IUserService
    {
        User Register(string email, string password, string? name = null, string? username = null);
        User? Login(string username, string password);
        User? GetByUsername(string username);
        User? GetByEmail(string email);
        User UpdateUser(Guid userId, string? newUsername, string? newPassword, string? newName = null);
        void DeleteUser(Guid userId);
        User? GetById(Guid userId);

        User? GetByPasswordResetToken(string token);

        void UpdatePassword(User user, string newPassword);

        // Create user for external auth (Google)
        User CreateExternalUser(string email, string? name, string provider);

        // Link a Google account to an existing user
        bool LinkGoogleAccount(Guid userId, string googleId);

        // Unlink a Google account from an existing user
        bool UnlinkGoogleAccount(Guid userId);

        User? GetByVerificationToken(string token);
        void Save(User user);
    }
}