using Domurion.Models;
using Domurion.Data;
using Domurion.Services.Interfaces;

namespace Domurion.Services
{
    public class FidoCredentialService(DataContext context) : IFidoCredentialService
    {
        private readonly DataContext _context = context;

        public List<FidoCredential> GetCredentialsByUserId(Guid userId)
        {
            return _context.Set<FidoCredential>().Where(c => c.UserId == userId.ToString()).ToList();
        }

        public FidoCredential? GetCredentialById(string credentialId)
        {
            var credentialIdBytes = Convert.FromBase64String(credentialId);
            return _context.Set<FidoCredential>().FirstOrDefault(c => c.CredentialId.SequenceEqual(credentialIdBytes));
        }
    }
}