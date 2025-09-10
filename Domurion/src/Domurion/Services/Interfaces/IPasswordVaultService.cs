using Domurion.Models;

namespace Domurion.Services.Interfaces
{
    public interface IPasswordVaultService
    {
        Credential AddCredential(Guid userId, string site, string username, string password, string? ipAddress = null);
        IEnumerable<Credential> GetCredentials(Guid userId);
        string RetrievePassword(Guid credentialId, Guid userId, string? ipAddress = null);
        Credential UpdateCredential(Guid credentialId, Guid userId, string? site, string? username, string? password, string? ipAddress = null);
        void DeleteCredential(Guid credentialId, Guid userId, string? ipAddress = null);

        // Password sharing
        Credential ShareCredential(Guid credentialId, Guid fromUserId, string toUsername, string? ipAddress = null);
    }
}