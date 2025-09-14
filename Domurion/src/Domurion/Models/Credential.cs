namespace Domurion.Models
{
    public class Credential
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        private string _site = string.Empty;
        public string Site
        {
            get => _site;
            set => _site = value ?? throw new ArgumentNullException(nameof(Site));
        }

        private string _username = string.Empty;
        public string Username
        {
            get => _username;
            set => _username = value ?? throw new ArgumentNullException(nameof(Username));
        }

        private string _encryptedPassword = string.Empty;
        public string EncryptedPassword
        {
            get => _encryptedPassword;
            set => _encryptedPassword = value ?? throw new ArgumentNullException(nameof(EncryptedPassword));
        }

        private string _integrityHash = string.Empty;
        public string IntegrityHash
        {
            get => _integrityHash;
            set => _integrityHash = value ?? throw new ArgumentNullException(nameof(IntegrityHash));
        }

        public Guid UserId { get; set; }

        public string? Notes { get; set; }
    }
}