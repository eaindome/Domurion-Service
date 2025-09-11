using Microsoft.Extensions.DependencyInjection;
using Domurion.Controllers;
using Domurion.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace Domurion.Tests
{
	public class GoogleLinkControllerTests
	{
		private static GoogleLinkController CreateController(IUserService? userService = null, HttpContext? httpContext = null, ClaimsPrincipal? user = null, bool withUrlHelper = false)
		{
			userService ??= Mock.Of<IUserService>();
			var controller = new GoogleLinkController(userService);
			var ctx = httpContext ?? new DefaultHttpContext();
			// Register a stub IAuthenticationService for all tests
			var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
			services.AddSingleton<IAuthenticationService>(new StubAuthenticationService());
			ctx.RequestServices = services.BuildServiceProvider();
			if (user != null) ctx.User = user;
			controller.ControllerContext = new ControllerContext { HttpContext = ctx };
			if (withUrlHelper)
				controller.Url = new StubUrlHelper();
			return controller;
		}

		[Fact]
		public void LinkGoogleOAuth_ReturnsChallengeResult()
		{
			var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()) }, "TestAuth"));
			var controller = CreateController(user: user, withUrlHelper: true);
			var result = controller.LinkGoogleOAuth("/return");
			var challenge = Assert.IsType<ChallengeResult>(result);
			Assert.Equal(GoogleDefaults.AuthenticationScheme, challenge.AuthenticationSchemes[0]);
		}
		// Stub IAuthenticationService for tests
		public class StubAuthenticationService : IAuthenticationService
		{
			public Task<AuthenticateResult> AuthenticateAsync(HttpContext context, string? scheme)
			{
				// Use the IAuthenticateResultFeature if set, else return NoResult
				var feature = context.Features.Get<IAuthenticateResultFeature>();
				if (feature != null && feature.AuthenticateResult != null)
					return Task.FromResult(feature.AuthenticateResult);
				return Task.FromResult(AuthenticateResult.NoResult());
			}
			public Task ChallengeAsync(HttpContext context, string? scheme, AuthenticationProperties? properties) => Task.CompletedTask;
			public Task ForbidAsync(HttpContext context, string? scheme, AuthenticationProperties? properties) => Task.CompletedTask;
			public Task SignInAsync(HttpContext context, string? scheme, ClaimsPrincipal principal, AuthenticationProperties? properties) => Task.CompletedTask;
			public Task SignOutAsync(HttpContext context, string? scheme, AuthenticationProperties? properties) => Task.CompletedTask;
		}

		// Simple stub for IUrlHelper
		public class StubUrlHelper : IUrlHelper
		{
			public ActionContext ActionContext => new();
			public static string? Action(string? action, string? controller, object? values) => "/link-google-callback";
			public string? Action(string? action, string? controller) => "/link-google-callback";
			public string? Action(string? action) => "/link-google-callback";
			public string? Content(string? contentPath) => contentPath;
			public bool IsLocalUrl(string? url) => true;
			public string? Link(string? routeName, object? values) => "/link-google-callback";
			public string? Action(object values) => "/link-google-callback";
			public string? RouteUrl(object values) => "/link-google-callback";
			public string? Action(Microsoft.AspNetCore.Mvc.Routing.UrlActionContext actionContext) => "/link-google-callback";
			public string? RouteUrl(Microsoft.AspNetCore.Mvc.Routing.UrlRouteContext routeContext) => "/link-google-callback";
		}

		[Fact]
		public async Task GoogleLinkCallback_Unauthorized_WhenNotAuthenticated()
		{
			var userService = Mock.Of<IUserService>();
			var httpContext = new DefaultHttpContext();
			// Simulate failed authentication
			var authResult = AuthenticateResult.Fail("fail");
			httpContext.Features.Set<IAuthenticateResultFeature>(new AuthenticateResultFeature(authResult));
			var controller = CreateController(userService, httpContext);
			var result = await controller.GoogleLinkCallback();
			Assert.IsType<UnauthorizedResult>(result);
		}

		[Fact]
		public async Task GoogleLinkCallback_BadRequest_WhenNoGoogleId()
		{
			var userService = Mock.Of<IUserService>();
			var httpContext = new DefaultHttpContext();
			// Simulate successful authentication but no Google ID
			var claims = new[] { new Claim(ClaimTypes.Name, "Test User") };
			var identity = new ClaimsIdentity(claims, GoogleDefaults.AuthenticationScheme);
			var principal = new ClaimsPrincipal(identity);
			var authResult = AuthenticateResult.Success(new AuthenticationTicket(principal, GoogleDefaults.AuthenticationScheme));
			httpContext.Features.Set<IAuthenticateResultFeature>(new AuthenticateResultFeature(authResult));
			// Simulate authenticated user
			var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()) }, "TestAuth"));
			var controller = CreateController(userService, httpContext, user);
			var result = await controller.GoogleLinkCallback();
			var badRequest = Assert.IsType<BadRequestObjectResult>(result);
			Assert.Contains("Google ID", badRequest.Value!.ToString(), System.StringComparison.OrdinalIgnoreCase);
		}

		[Fact]
		public async Task GoogleLinkCallback_Unauthorized_WhenNoUserId()
		{
			var userService = Mock.Of<IUserService>();
			var httpContext = new DefaultHttpContext();
			// Simulate successful authentication with Google ID
			var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "googleid123") };
			var identity = new ClaimsIdentity(claims, GoogleDefaults.AuthenticationScheme);
			var principal = new ClaimsPrincipal(identity);
			var authResult = AuthenticateResult.Success(new AuthenticationTicket(principal, GoogleDefaults.AuthenticationScheme));
			httpContext.Features.Set<IAuthenticateResultFeature>(new AuthenticateResultFeature(authResult));
			// No user in JWT
			var controller = CreateController(userService, httpContext);
			var result = await controller.GoogleLinkCallback();
			Assert.IsType<UnauthorizedResult>(result);
		}

		[Fact]
		public async Task GoogleLinkCallback_Success_ReturnsOk()
		{
			var userId = Guid.NewGuid();
			var userService = new Mock<IUserService>();
			userService.Setup(s => s.LinkGoogleAccount(userId, "googleid123")).Returns(true);
			var httpContext = new DefaultHttpContext();
			// Simulate successful authentication with Google ID
			var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "googleid123") };
			var identity = new ClaimsIdentity(claims, GoogleDefaults.AuthenticationScheme);
			var principal = new ClaimsPrincipal(identity);
			var authResult = AuthenticateResult.Success(new AuthenticationTicket(principal, GoogleDefaults.AuthenticationScheme));
			httpContext.Features.Set<IAuthenticateResultFeature>(new AuthenticateResultFeature(authResult));
			// Simulate authenticated user
			var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) }, "TestAuth"));
			var controller = CreateController(userService.Object, httpContext, user);
			var result = await controller.GoogleLinkCallback();
			var ok = Assert.IsType<OkObjectResult>(result);
			Assert.Contains("linked", ok.Value!.ToString(), System.StringComparison.OrdinalIgnoreCase);
		}

		[Fact]
		public async Task GoogleLinkCallback_AlreadyLinked_ReturnsBadRequest()
		{
			var userId = Guid.NewGuid();
			var userService = new Mock<IUserService>();
			userService.Setup(s => s.LinkGoogleAccount(userId, "googleid123")).Returns(false);
			var httpContext = new DefaultHttpContext();
			// Simulate successful authentication with Google ID
			var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "googleid123") };
			var identity = new ClaimsIdentity(claims, GoogleDefaults.AuthenticationScheme);
			var principal = new ClaimsPrincipal(identity);
			var authResult = AuthenticateResult.Success(new AuthenticationTicket(principal, GoogleDefaults.AuthenticationScheme));
			httpContext.Features.Set<IAuthenticateResultFeature>(new AuthenticateResultFeature(authResult));
			// Simulate authenticated user
			var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) }, "TestAuth"));
			var controller = CreateController(userService.Object, httpContext, user);
			var result = await controller.GoogleLinkCallback();
			var badRequest = Assert.IsType<BadRequestObjectResult>(result);
			Assert.Contains("already linked", badRequest.Value!.ToString(), System.StringComparison.OrdinalIgnoreCase);
		}

		[Fact]
		public async Task GoogleLinkCallback_Redirects_WhenReturnUrlProvided()
		{
			var userId = Guid.NewGuid();
			var userService = new Mock<IUserService>();
			userService.Setup(s => s.LinkGoogleAccount(userId, "googleid123")).Returns(true);
			var httpContext = new DefaultHttpContext();
			// Simulate successful authentication with Google ID
			var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "googleid123") };
			var identity = new ClaimsIdentity(claims, GoogleDefaults.AuthenticationScheme);
			var principal = new ClaimsPrincipal(identity);
			var authResult = AuthenticateResult.Success(new AuthenticationTicket(principal, GoogleDefaults.AuthenticationScheme));
			httpContext.Features.Set<IAuthenticateResultFeature>(new AuthenticateResultFeature(authResult));
			// Simulate authenticated user
			var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) }, "TestAuth"));
			var controller = CreateController(userService.Object, httpContext, user);
			var result = await controller.GoogleLinkCallback("/profile");
			var redirect = Assert.IsType<RedirectResult>(result);
			Assert.StartsWith("/profile?linked=", redirect.Url);
		}

		[Fact]
		public void UnlinkGoogle_Success_ReturnsOk()
		{
			var userId = Guid.NewGuid();
			var userService = new Mock<IUserService>();
			userService.Setup(s => s.UnlinkGoogleAccount(userId)).Returns(true);
			var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) }, "TestAuth"));
			var controller = CreateController(userService.Object, user: user);
			var result = controller.UnlinkGoogle();
			var ok = Assert.IsType<OkObjectResult>(result);
			Assert.Contains("unlinked", ok.Value!.ToString(), System.StringComparison.OrdinalIgnoreCase);
		}

		[Fact]
		public void UnlinkGoogle_NotLinked_ReturnsBadRequest()
		{
			var userId = Guid.NewGuid();
			var userService = new Mock<IUserService>();
			userService.Setup(s => s.UnlinkGoogleAccount(userId)).Returns(false);
			var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) }, "TestAuth"));
			var controller = CreateController(userService.Object, user: user);
			var result = controller.UnlinkGoogle();
			var badRequest = Assert.IsType<BadRequestObjectResult>(result);
			Assert.Contains("No Google account", badRequest.Value!.ToString(), System.StringComparison.OrdinalIgnoreCase);
		}

		[Fact]
		public void UnlinkGoogle_Unauthorized_WhenNoUser()
		{
			var userService = Mock.Of<IUserService>();
			var controller = CreateController(userService);
			var result = controller.UnlinkGoogle();
			Assert.IsType<UnauthorizedResult>(result);
		}

		// Helper for setting authentication result in HttpContext
		private class AuthenticateResultFeature : IAuthenticateResultFeature
		{
			public AuthenticateResultFeature(AuthenticateResult? result) => AuthenticateResult = result;
			public AuthenticateResult? AuthenticateResult { get; set; }
		}
	}
}
