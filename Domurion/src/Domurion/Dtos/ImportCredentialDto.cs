namespace Domurion.Dtos
{
    public class ImportCredentialDto
    {
        public required string Site { get; set; }
        public required string Username { get; set; }
        public required string Password { get; set; }
    }
}