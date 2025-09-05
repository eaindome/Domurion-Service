using Domurion.Models;

namespace Domurion.Services.Interfaces
{
    public interface IFidoCredentialService
    {
        List<FidoCredential> GetCredentialsByUserId(Guid userId);
    }
}