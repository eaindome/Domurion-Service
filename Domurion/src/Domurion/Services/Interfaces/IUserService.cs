using Domurion.Models;

namespace Domurion.Services.Interfaces
{
    public interface IUserService
    {
        User Register(string username, string password, string? name = null);
        User? Login(string username, string password);
        User? GetByUsername(string username);
        User UpdateUser(Guid userId, string? newUsername, string? newPassword, string? newName = null);
        void DeleteUser(Guid userId);
        User? GetById(Guid userId);

        // Create user for external auth (Google)
        User CreateExternalUser(string email, string? name, string provider);

        // Link a Google account to an existing user
        bool LinkGoogleAccount(Guid userId, string googleId);

        // Unlink a Google account from an existing user
        bool UnlinkGoogleAccount(Guid userId);
    }
}