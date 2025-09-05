using Domurion.Models;

namespace Domurion.Services
{
    public interface IUserService
    {
        User Register(string username, string password);
        User? Login(string username, string password);
        User? GetByUsername(string username);
    }
}