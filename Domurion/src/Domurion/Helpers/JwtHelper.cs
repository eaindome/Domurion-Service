using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Domurion.Models;

namespace Domurion.Helpers
{
    public static class JwtHelper
    {
        public static string GenerateJwtToken(User user, IConfiguration configuration, UserPreferences? preferences = null)
        {
            var jwtKey = configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
                throw new ArgumentNullException(nameof(jwtKey), "JWT Key is missing in configuration.");
            var jwtIssuer = configuration["Jwt:Issuer"];
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Determine expiration from preferences (SessionTimeoutMinutes), fallback to 12 hours
            int? sessionTimeout = preferences?.SessionTimeoutMinutes;
            DateTime expires = DateTime.UtcNow.AddHours(12);
            if (sessionTimeout.HasValue && sessionTimeout.Value > 0)
            {
                expires = DateTime.UtcNow.AddMinutes(sessionTimeout.Value);
            }

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: null,
                claims: claims,
                expires: expires,
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
