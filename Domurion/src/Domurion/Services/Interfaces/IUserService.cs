using Domurion.Models;

namespace Domurion.Services.Interfaces
{
    public interface IUserService
    {
        User Register(string username, string password);
        User? Login(string username, string password);
        User? GetByUsername(string username);
        User UpdateUser(Guid userId, string? newUsername, string? newPassword);
        void DeleteUser(Guid userId);
    }
}