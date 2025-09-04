using Domurion.Models;

namespace Domurion.Services.Interfaces
{
    public interface IPasswordVaultService
    {
        Credential AddCredential(Guid userId, string site, string username, string password);
        IEnumerable<Credential> GetCredentials(Guid userId);
        string RetrievePassword(Guid credentialId, Guid userId);
    }
}