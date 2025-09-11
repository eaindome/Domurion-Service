using Domurion.Helpers;
using Domurion.Models;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Xunit;

namespace Domurion.Tests
{
	public class JwtHelperTests
	{
	private static IConfiguration CreateConfig(string key = "supersecretkey12345678901234567890", string issuer = "testissuer")
		{
			var dict = new Dictionary<string, string?>
			{
				{ "Jwt:Key", key },
				{ "Jwt:Issuer", issuer }
			};
			return new ConfigurationBuilder().AddInMemoryCollection(dict).Build();
		}

		[Fact]
		public void GenerateJwtToken_ProducesValidToken_WithClaims()
		{
			var user = new User { Id = Guid.NewGuid(), Username = "testuser" };
			var config = CreateConfig();
			var token = JwtHelper.GenerateJwtToken(user, config);
			Assert.False(string.IsNullOrWhiteSpace(token));
			var handler = new JwtSecurityTokenHandler();
			var jwt = handler.ReadJwtToken(token);
			Assert.Equal(user.Id.ToString(), jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value);
			Assert.Equal(user.Username, jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.UniqueName).Value);
			Assert.Equal("testissuer", jwt.Issuer);
		}

		[Fact]
		public void GenerateJwtToken_RespectsSessionTimeout()
		{
			var user = new User { Id = Guid.NewGuid(), Username = "testuser" };
			var config = CreateConfig();
			var prefs = new UserPreferences { SessionTimeoutMinutes = 10 };
			var token = JwtHelper.GenerateJwtToken(user, config, prefs);
			var handler = new JwtSecurityTokenHandler();
			var jwt = handler.ReadJwtToken(token);
			var exp = jwt.ValidTo;
			var now = DateTime.UtcNow;
			Assert.InRange((exp - now).TotalMinutes, 9, 11); // Allow 1 min clock skew
		}

		[Fact]
		public void GenerateJwtToken_ThrowsIfKeyMissing()
		{
			var user = new User { Id = Guid.NewGuid(), Username = "testuser" };
			var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { { "Jwt:Issuer", "issuer" } }).Build();
			Assert.Throws<ArgumentNullException>(() => JwtHelper.GenerateJwtToken(user, config));
		}

		[Fact]
		public void GenerateJwtToken_UsesDefaultExpiryIfNoPrefs()
		{
			var user = new User { Id = Guid.NewGuid(), Username = "testuser" };
			var config = CreateConfig();
			var token = JwtHelper.GenerateJwtToken(user, config, null);
			var handler = new JwtSecurityTokenHandler();
			var jwt = handler.ReadJwtToken(token);
			var exp = jwt.ValidTo;
			var now = DateTime.UtcNow;
			Assert.InRange((exp - now).TotalHours, 11, 13); // 12h +/- 1h
		}
	}
}
