
namespace Domurion.Models
{
    public class FidoCredential
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string UserId { get; set; } = string.Empty;
        public byte[] CredentialId { get; set; } = [];
        public byte[] PublicKey { get; set; } = [];
        public uint SignatureCounter { get; set; }
        public string CredType { get; set; } = string.Empty;
        public string RegDate { get; set; } = string.Empty;
        public string AaGuid { get; set; } = string.Empty;
    }
}