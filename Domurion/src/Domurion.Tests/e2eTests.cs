using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace Domurion.Tests
{
    public class e2eTests
    {
        private static readonly string DbName = "TestDb-Shared";
        private static readonly ServiceProvider SharedProvider = new ServiceCollection()
            .AddEntityFrameworkInMemoryDatabase()
            .BuildServiceProvider();

        [Fact(Skip = "e2e tests skipped for now")]
        public async Task Register_Login_JWT_ProtectedEndpoint_Flow()
        {
            var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, configBuilder) =>
                {
                    var dict = new Dictionary<string, string?>
                    {
                        {"Jwt:Key", "THIS_IS_A_TEST_KEY_FOR_E2E_TESTS_12345678901234567890"},
                        {"Jwt:Issuer", "TestIssuer"},
                        {"Authentication:Google:ClientId", "test-google-client-id"},
                        {"Authentication:Google:ClientSecret", "test-google-client-secret"},
                        {"ConnectionStrings:DefaultConnection", "Fake"}
                    };
                    configBuilder.AddInMemoryCollection(dict);
                });
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<Domurion.Data.DataContext>));
                    if (descriptor != null)
                        services.Remove(descriptor);
                    services.AddDbContext<Domurion.Data.DataContext>(options =>
                    {
                        options.UseInMemoryDatabase(DbName)
                               .UseInternalServiceProvider(SharedProvider);
                    });
                });
            });
            var client = factory.CreateClient();
            var username = $"user{System.Guid.NewGuid()}@test.com";
            var password = "TestPassword123!";
            // Register
            var regResp = await client.PostAsJsonAsync("/api/users/register", new { Username = username, Password = password });
            Assert.Equal(HttpStatusCode.OK, regResp.StatusCode);
            var regJson = await regResp.Content.ReadFromJsonAsync<JsonElement>();
            var userId = regJson.GetProperty("id").GetString();
            // Login
            var loginResp = await client.PostAsJsonAsync("/api/users/login", new { Username = username, Password = password });
            Assert.Equal(HttpStatusCode.OK, loginResp.StatusCode);
            var loginJson = await loginResp.Content.ReadFromJsonAsync<JsonElement>();
            var token = loginJson.GetProperty("token").GetString();
            Assert.False(string.IsNullOrEmpty(token));

            // Debug: Print token and decode claims
            System.Console.WriteLine($"JWT Token: {token}");
            var parts = token.Split('.');
            if (parts.Length == 3)
            {
                var payload = parts[1];
                var padded = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
                var json = System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(padded.Replace('-', '+').Replace('_', '/')));
                System.Console.WriteLine($"JWT Payload: {json}");
            }

            // Access protected endpoint
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var protectedResp = await client.PostAsync("/api/users/link-google", JsonContent.Create("fake-google-id"));
            Assert.NotEqual(HttpStatusCode.Unauthorized, protectedResp.StatusCode); // Should be authorized
        }


        [Fact(Skip = "e2e tests skipped for now")]
        public async Task TwoFactor_Enable_Verify_Disable_Flow()
        {
            var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, configBuilder) =>
                {
                    var dict = new Dictionary<string, string?>
                    {
                        {"Jwt:Key", "THIS_IS_A_TEST_KEY_FOR_E2E_TESTS_12345678901234567890"},
                        {"Jwt:Issuer", "TestIssuer"},
                        {"Authentication:Google:ClientId", "test-google-client-id"},
                        {"Authentication:Google:ClientSecret", "test-google-client-secret"},
                        {"ConnectionStrings:DefaultConnection", "Fake"}
                    };
                    configBuilder.AddInMemoryCollection(dict);
                });
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<Domurion.Data.DataContext>));
                    if (descriptor != null)
                        services.Remove(descriptor);
                    services.AddDbContext<Domurion.Data.DataContext>(options =>
                    {
                        options.UseInMemoryDatabase(DbName)
                               .UseInternalServiceProvider(SharedProvider);
                    });
                });
            });
            var client = factory.CreateClient();
            var username = $"user2fa{System.Guid.NewGuid()}@test.com";
            var password = "TestPassword123!";
            // Register
            var regResp = await client.PostAsJsonAsync("/api/users/register", new { Username = username, Password = password });
            Assert.Equal(HttpStatusCode.OK, regResp.StatusCode);
            // Login
            var loginResp = await client.PostAsJsonAsync("/api/users/login", new { Username = username, Password = password });
            Assert.Equal(HttpStatusCode.OK, loginResp.StatusCode);
            var loginJson = await loginResp.Content.ReadFromJsonAsync<JsonElement>();
            var token = loginJson.GetProperty("token").GetString();
            Assert.False(string.IsNullOrEmpty(token));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            // Enable 2FA
            var enableResp = await client.PostAsync("/api/2fa/enable", null);
            Assert.Equal(HttpStatusCode.OK, enableResp.StatusCode);
            var enableJson = await enableResp.Content.ReadFromJsonAsync<JsonElement>();
            var secret = enableJson.GetProperty("secret").GetString();
            Assert.False(string.IsNullOrEmpty(secret));
            // Generate TOTP code
            var totp = new OtpNet.Totp(OtpNet.Base32Encoding.ToBytes(secret));
            var code = totp.ComputeTotp();
            // Verify 2FA
            var verifyResp = await client.PostAsJsonAsync("/api/2fa/verify", code);
            Assert.Equal(HttpStatusCode.OK, verifyResp.StatusCode);
            // Disable 2FA
            var disableResp = await client.PostAsJsonAsync("/api/2fa/disable", code);
            Assert.Equal(HttpStatusCode.OK, disableResp.StatusCode);
        }


        [Fact(Skip = "Google OAuth flow requires external provider simulation or advanced test server setup.")]
        public void GoogleOAuth_Login_And_Link_Flow()
        {
            // This test is scaffolded. To implement, you would need to mock Google authentication or use a test double for the external provider.
            // You can use TestServer with a custom authentication handler to simulate Google login if needed.
        }
    }
}
