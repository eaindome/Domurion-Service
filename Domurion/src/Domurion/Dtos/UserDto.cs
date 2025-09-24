namespace Domurion.Dtos
{
    public class UserDto
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
        public string? Name { get; set; }
        public string? Username { get; set; }
    }

    public class UpdateUserDto
    {
        public string? NewUsername { get; set; }
        public string? NewPassword { get; set; }
        public string? Name { get; set; }
    }
}
