using Microsoft.Extensions.Configuration;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace Domurion.Tests
{
    public class AuthProtectedTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private const string TestUserId = "test-user-id";
        private const string TestUserEmail = "test@example.com";
        private const string JwtKey = "super_secret_test_key_1234567890";
        private const string JwtIssuer = "test-issuer";

        public AuthProtectedTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, configBuilder) =>
                {
                    // Remove all previous config sources so our in-memory config is highest priority
                    configBuilder.Sources.Clear();
                    var dict = new Dictionary<string, string?>
                    {
                        {"Jwt:Key", JwtKey},
                        {"Jwt:Issuer", JwtIssuer},
                        {"Jwt:Audience", "test-audience"},
                        {"Authentication:Google:ClientId", "dummy-client-id"},
                        {"Authentication:Google:ClientSecret", "dummy-client-secret"}
                    };
                    configBuilder.AddInMemoryCollection(dict);
                });
                builder.ConfigureServices(services =>
                {
                    services.AddControllers().AddApplicationPart(typeof(TestAuthController).Assembly);
                    // Add a post-configure action to log the JWT config at runtime for debugging
                    services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
                    {
                        var sp = services.BuildServiceProvider();
                        var config = sp.GetRequiredService<IConfiguration>();
                        System.Diagnostics.Debug.WriteLine($"[TEST DEBUG] Jwt:Key = {config["Jwt:Key"]}, Jwt:Issuer = {config["Jwt:Issuer"]}, Jwt:Audience = {config["Jwt:Audience"]}");
                    });
                });
            });
        }

        [Fact]
        public async Task AuthorizeEndpoint_Returns401_WithoutToken()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/auth-protected");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task AuthorizeEndpoint_Succeeds_WithValidJwt()
        {
            var client = _factory.CreateClient();
            var token = GenerateJwtToken();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await client.GetAsync("/auth-protected");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var json = await response.Content.ReadAsStringAsync();
            Assert.Contains(TestUserId, json);
            Assert.Contains(TestUserEmail, json);
        }

        [Fact]
        public async Task AuthorizeEndpoint_UserClaims_Accessible()
        {
            var client = _factory.CreateClient();
            var token = GenerateJwtToken();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await client.GetAsync("/auth-protected");
            var json = await response.Content.ReadAsStringAsync();
            Assert.Contains(TestUserId, json);
            Assert.Contains(TestUserEmail, json);
        }

        private string GenerateJwtToken()
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, TestUserId),
                new Claim(ClaimTypes.Email, TestUserEmail)
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: JwtIssuer,
                audience: null,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    [ApiController]
    [Route("/auth-protected")]
    public class TestAuthController : ControllerBase
    {
        [HttpGet]
        [Authorize]
        public IActionResult Get()
        {
            if (!User.Identity?.IsAuthenticated ?? true)
                return Unauthorized();
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            return Ok(new { userId, email });
        }
    }
}
