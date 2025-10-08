namespace Domurion.Dtos
{
    public class ImportCredentialDto
    {
        // Make every field optional because different exporters include different fields
        public string? Site { get; set; }
        public string? SiteUrl { get; set; }
        // Either Username or Email may be present in imports from different tools
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? Notes { get; set; }
    }
}