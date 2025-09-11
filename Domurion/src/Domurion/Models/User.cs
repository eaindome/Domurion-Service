namespace Domurion.Models
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string? Name { get; set; }

        private string _username = string.Empty;
        public string Username
        {
            get => _username;
            set => _username = value ?? throw new ArgumentNullException(nameof(Username));
        }

        private string _passwordHash = string.Empty;
        public string PasswordHash
        {
            get => _passwordHash;
            set => _passwordHash = value ?? throw new ArgumentNullException(nameof(PasswordHash));
        }

        public string? AuthProvider { get; set; }

        // For account linking: store Google unique ID if linked
        public string? GoogleId { get; set; }
    }
}