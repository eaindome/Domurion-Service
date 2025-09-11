namespace Domurion.Models
{
    public class UserPreferences
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public int PasswordLength { get; set; } = 16;
        public bool UseUppercase { get; set; } = true;
        public bool UseLowercase { get; set; } = true;
        public bool UseNumbers { get; set; } = true;
        public bool UseSymbols { get; set; } = true;
        public bool AutoSaveEntries { get; set; } = false;
        public bool ShowPasswordStrength { get; set; } = true;
        public int? SessionTimeoutMinutes { get; set; } // null or 0 = never
    }
}