namespace Domurion.Models
{
    public class SupportRequest
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid? UserId { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Reason { get; set; }
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public bool Resolved { get; set; } = false;
        public DateTime? ResolvedAt { get; set; }
        public string? ResolutionNote { get; set; }
    }
}
