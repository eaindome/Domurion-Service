namespace Domurion.Models
{
    public class Credential
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Site { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string EncryptedPassword { get; set; } = string.Empty;
        public string IntegrityHash { get; set; } = string.Empty;
        public Guid UserId { get; set; }
    }
}