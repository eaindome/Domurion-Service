using Domurion.Controllers;
using Domurion.Models;
using Domurion.Services;
using Domurion.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace Domurion.Tests
{
    public class AuthControllerTests
    {
        private static AuthController CreateController(
            IUserService? userService = null,
            IConfiguration? config = null,
            IPreferencesService? preferencesService = null,
            HttpContext? httpContext = null,
            AuthenticateResult? authResult = null)
        {
            userService ??= Mock.Of<IUserService>();
            if (config == null)
            {
                // Use a key of at least 33 bytes (264 bits) for HS256 (library now requires >256 bits)
                var dict = new Dictionary<string, string?>
                {
                    { "Jwt:Key", "dummy-test-key-abcdefghijklmnopqrstuvwxyz123456" }
                };
                config = new ConfigurationBuilder().AddInMemoryCollection(dict).Build();
            }
            preferencesService ??= Mock.Of<IPreferencesService>();
            if (httpContext == null)
            {
                httpContext = new DefaultHttpContext();
            }
            // Always register a stub IAuthenticationService
            var services = new ServiceCollection();
            services.AddSingleton<IAuthenticationService>(new StubAuthenticationService(authResult));
            httpContext.RequestServices = services.BuildServiceProvider();
            var controller = new AuthController(userService, config, preferencesService);
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            return controller;
        }

        [Fact]
        public void GoogleLogin_ReturnsChallengeResult()
        {
            var controller = CreateController();
            // Assign a stub UrlHelper so Url.Action does not throw
            controller.Url = new StubUrlHelper();
            var result = controller.GoogleLogin("/return");
            var challenge = Assert.IsType<ChallengeResult>(result);
            Assert.Equal(GoogleDefaults.AuthenticationScheme, challenge.AuthenticationSchemes[0]);
        }

        [Fact]
        public async Task GoogleResponse_Unauthorized_WhenNotAuthenticated()
        {
            var userService = Mock.Of<IUserService>();
            var config = new ConfigurationBuilder().AddInMemoryCollection().Build();
            var prefsService = Mock.Of<IPreferencesService>();
            var httpContext = new DefaultHttpContext();
            // Simulate failed authentication
            var authResult = AuthenticateResult.Fail("fail");
            var controller = CreateController(userService, config, prefsService, httpContext, authResult);
            var result = await controller.GoogleResponse();
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task GoogleResponse_BadRequest_WhenNoEmail()
        {
            var userService = Mock.Of<IUserService>();
            var config = new ConfigurationBuilder().AddInMemoryCollection().Build();
            var prefsService = Mock.Of<IPreferencesService>();
            var httpContext = new DefaultHttpContext();
            // Simulate successful authentication but no email
            var claims = new[] { new Claim(ClaimTypes.Name, "Test User") };
            var identity = new ClaimsIdentity(claims, GoogleDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            var authResult = AuthenticateResult.Success(new AuthenticationTicket(principal, GoogleDefaults.AuthenticationScheme));
            var controller = CreateController(userService, config, prefsService, httpContext, authResult);
            var result = await controller.GoogleResponse();
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequest.Value);
            Assert.Contains("email", badRequest.Value.ToString(), System.StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GoogleResponse_Success_ReturnsToken()
        {
            var user = new User { Id = Guid.NewGuid(), Username = "test@example.com", Name = "Test User" };
            var userService = new Mock<IUserService>();
            userService.Setup(s => s.GetByUsername(user.Username)).Returns((User?)null);
            userService.Setup(s => s.CreateExternalUser(user.Username, user.Name, "Google")).Returns(user);
            // Use default config with Jwt:Key
            // Use mock IPreferencesService so GetPreferences does not hit the database
            var prefsService = new Mock<IPreferencesService>();
            prefsService.Setup(p => p.GetPreferences(It.IsAny<Guid>())).Returns((Guid id) => new UserPreferences { UserId = id });
            // Simulate successful authentication with email and name
            var claims = new[] {
                   new Claim(ClaimTypes.Email, user.Username),
                   new Claim(ClaimTypes.Name, user.Name)
               };
            var identity = new ClaimsIdentity(claims, GoogleDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            var authResult = AuthenticateResult.Success(new AuthenticationTicket(principal, GoogleDefaults.AuthenticationScheme));
            var httpContext = new DefaultHttpContext();
            var controller = CreateController(userService.Object, null, prefsService.Object, httpContext, authResult);
            var result = await controller.GoogleResponse();
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(ok.Value);
            Assert.NotNull(ok.Value.GetType().GetProperty("token")?.GetValue(ok.Value, null));
        }

        [Fact]
        public async Task GoogleResponse_Redirects_WhenReturnUrlProvided()
        {
            var user = new User { Id = Guid.NewGuid(), Username = "test@example.com", Name = "Test User" };
            var userService = new Mock<IUserService>();
            userService.Setup(s => s.GetByUsername(user.Username)).Returns(user);
            // Use default config with Jwt:Key
            // Use mock IPreferencesService so GetPreferences does not hit the database
            var prefsService = new Mock<IPreferencesService>();
            prefsService.Setup(p => p.GetPreferences(It.IsAny<Guid>())).Returns((Guid id) => new UserPreferences { UserId = id });
            // Simulate successful authentication with email and name
            var claims = new[] {
                   new Claim(ClaimTypes.Email, user.Username),
                   new Claim(ClaimTypes.Name, user.Name)
               };
            var identity = new ClaimsIdentity(claims, GoogleDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            var authResult = AuthenticateResult.Success(new AuthenticationTicket(principal, GoogleDefaults.AuthenticationScheme));
            var httpContext = new DefaultHttpContext();
            var controller = CreateController(userService.Object, null, prefsService.Object, httpContext, authResult);
            var result = await controller.GoogleResponse("/home");
            var redirect = Assert.IsType<RedirectResult>(result);
            Assert.StartsWith("/home?token=", redirect.Url);
        }
        // Stub IAuthenticationService for tests
        public class StubAuthenticationService : IAuthenticationService
        {
            private readonly AuthenticateResult? _result;
            public StubAuthenticationService(AuthenticateResult? result) => _result = result;
            public Task<AuthenticateResult> AuthenticateAsync(HttpContext context, string? scheme)
                => Task.FromResult(_result ?? AuthenticateResult.NoResult());
            public Task ChallengeAsync(HttpContext context, string? scheme, AuthenticationProperties? properties) => Task.CompletedTask;
            public Task ForbidAsync(HttpContext context, string? scheme, AuthenticationProperties? properties) => Task.CompletedTask;
            public Task SignInAsync(HttpContext context, string? scheme, ClaimsPrincipal principal, AuthenticationProperties? properties) => Task.CompletedTask;
            public Task SignOutAsync(HttpContext context, string? scheme, AuthenticationProperties? properties) => Task.CompletedTask;
        }

        // Helper for setting authentication result in HttpContext
        public class AuthenticateResultFeature(AuthenticateResult result) : IAuthenticateResultFeature
        {
            public AuthenticateResult? AuthenticateResult { get; set; } = result;
        }
    }


    // Simple stub for IUrlHelper
    public class StubUrlHelper : IUrlHelper
    {
        public ActionContext ActionContext => new();
        public static string? Action(string? action, string? controller, object? values) => "/signin-google";
        public string? Action(string? action, string? controller) => throw new NotImplementedException();
        public string? Action(string? action) => throw new NotImplementedException();
        public string? Content(string? contentPath) => contentPath;
        public bool IsLocalUrl(string? url) => true;
        public string? Link(string? routeName, object? values) => throw new NotImplementedException();
        public string? Action(object values) => throw new NotImplementedException();
        public string? RouteUrl(object values) => throw new NotImplementedException();

        // Implement missing interface members
        public string? Action(Microsoft.AspNetCore.Mvc.Routing.UrlActionContext actionContext) => "/signin-google";
        public string? RouteUrl(Microsoft.AspNetCore.Mvc.Routing.UrlRouteContext routeContext) => "/signin-google";
    }
}

