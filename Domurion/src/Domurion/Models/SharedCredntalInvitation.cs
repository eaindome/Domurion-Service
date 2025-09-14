namespace Domurion.Models
{
    public class SharedCredentialInvitation
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid CredentialId { get; set; }
        public Guid FromUserId { get; set; }
        public Guid ToUserId { get; set; }
        public string ToEmail { get; set; } = string.Empty; // For lookup by email
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool Accepted { get; set; } = false;
        public bool Rejected { get; set; } = false;
        public DateTime? RespondedAt { get; set; }
    }
}