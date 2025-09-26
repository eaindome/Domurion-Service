namespace Domurion.Models
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string? Name { get; set; }
        private string _email = string.Empty;
        public string Email
        {
            get => _email;
            set => _email = value ?? throw new ArgumentNullException(nameof(Email));
        }

        public string Username { get; set; } = string.Empty;

        private string _passwordHash = string.Empty;
        public string PasswordHash
        {
            get => _passwordHash;
            set => _passwordHash = value ?? throw new ArgumentNullException(nameof(PasswordHash));
        }

        public string? ProfilePictureUrl { get; set; }

        public string? AuthProvider { get; set; }

        // For account linking: store Google unique ID if linked
        public string? GoogleId { get; set; }

        // Two-Factor Authentication (2FA)
        public bool TwoFactorEnabled { get; set; } = false;
        public string? TwoFactorSecret { get; set; }
        public string? TwoFactorRecoveryCodes { get; set; } // Comma-separated or JSON array

        public bool EmailVerified { get; set; } = false;
        public string? EmailVerificationToken { get; set; }
        public DateTime? EmailVerificationTokenExpiresAt { get; set; }
        public DateTime? LastVerificationEmailSentAt { get; set; }

        public string? PendingOtp { get; set; }
        public DateTime? PendingOtpExpiresAt { get; set; }

        // Password reset functionality
        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpiresAt { get; set; }
    }
}