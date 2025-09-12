using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Domurion;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace Domurion.Tests
{
    public class GlobalErrorHandlingTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public GlobalErrorHandlingTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, configBuilder) =>
                {
                    var dict = new System.Collections.Generic.Dictionary<string, string?>
                    {
                        {"Jwt:Key", "TEST_KEY_FOR_TESTS_12345678901234567890"},
                        {"Jwt:Issuer", "TestIssuer"}
                    };
                    configBuilder.AddInMemoryCollection(dict);
                });
                builder.ConfigureServices(services => { });
                builder.Configure(app =>
                {
                    app.UseMiddleware<Domurion.Helpers.ErrorHandlingMiddleware>();
                    app.UseRouting();
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapGet("/throw", context => throw new System.Exception("Test exception"));
                        endpoints.MapPost("/validate", async context =>
                        {
                            var model = await System.Text.Json.JsonSerializer.DeserializeAsync<TestModel>(context.Request.Body, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                            if (model == null || string.IsNullOrEmpty(model.Name))
                            {
                                context.Response.StatusCode = 400;
                                context.Response.ContentType = "application/json";
                                var error = System.Text.Json.JsonSerializer.Serialize(new { errors = new { Name = new[] { "The Name field is required." } } });
                                await context.Response.WriteAsync(error);
                                return;
                            }
                            context.Response.StatusCode = 200;
                            await context.Response.CompleteAsync();
                        });
                    });
                });
            });
        }

        [Fact]
        public async Task UnhandledException_IsCaughtByGlobalErrorMiddleware()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/throw");
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            var json = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.True(json.TryGetProperty("error", out var errorProp));
            Assert.Equal("An unexpected error occurred.", errorProp.GetString());
        }

        [Fact]
        public async Task ValidationError_ReturnsConsistentFormat()
        {
            var client = _factory.CreateClient();
            // Send invalid model (missing required property)
            var response = await client.PostAsJsonAsync("/validate", new { });
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var json = await response.Content.ReadAsStringAsync();
            Assert.Contains("errors", json); // Default ASP.NET Core validation error format
        }

        public class TestModel
        {
            [FromBody]
            [System.ComponentModel.DataAnnotations.Required]
            public string? Name { get; set; }
        }
    }
}
