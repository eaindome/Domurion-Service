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
                builder.ConfigureServices(services =>
                {
                    // Override JWT settings for test
                    services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
                    {
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidateAudience = false,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,
                            ValidIssuer = JwtIssuer,
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey))
                        };
                    });
                });
                builder.Configure(app =>
                {
                    app.UseRouting();
                    app.UseAuthentication();
                    app.UseAuthorization();
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapGet("/auth-protected", async context =>
                        {
                            if (!context.User.Identity?.IsAuthenticated ?? true)
                            {
                                context.Response.StatusCode = 401;
                                return;
                            }
                            var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                            var email = context.User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                            context.Response.ContentType = "application/json";
                            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new { userId, email }));
                        }).RequireAuthorization();
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
}
