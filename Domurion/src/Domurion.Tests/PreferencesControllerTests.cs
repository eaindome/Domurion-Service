using Domurion.Controllers;
using Domurion.Models;
using Domurion.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Domurion.Tests
{
	public class PreferencesControllerTests
	{
		private static Data.DataContext CreateInMemoryContext()
		{
			var options = new DbContextOptionsBuilder<Data.DataContext>()
				.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
				.Options;
			return new Data.DataContext(options);
		}

		private static PreferencesController CreateController(Data.DataContext context, Guid userId)
		{
			var service = new PreferencesService(context);
			var controller = new PreferencesController(service);
			// Mock authenticated user
			var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
			var identity = new ClaimsIdentity(claims, "TestAuth");
			var principal = new ClaimsPrincipal(identity);
			controller.ControllerContext = new ControllerContext
			{
				HttpContext = new DefaultHttpContext { User = principal }
			};
			return controller;
		}

		[Fact]
		public void Get_ReturnsDefaultPreferences_WhenNoneExist()
		{
			var context = CreateInMemoryContext();
			var userId = Guid.NewGuid();
			var controller = CreateController(context, userId);
			var result = controller.Get();
			var ok = Assert.IsType<OkObjectResult>(result.Result);
			var prefs = Assert.IsType<UserPreferences>(ok.Value);
			Assert.Equal(userId, prefs.UserId);
		}

		[Fact]
		public void Update_CreatesAndUpdatesPreferences()
		{
			var context = CreateInMemoryContext();
			var userId = Guid.NewGuid();
			var controller = CreateController(context, userId);
			var updated = new UserPreferences
			{
				PasswordLength = 24,
				UseUppercase = false,
				UseLowercase = true,
				UseNumbers = false,
				UseSymbols = true,
				AutoSaveEntries = true,
				ShowPasswordStrength = true,
				SessionTimeoutMinutes = 60
			};
			var result = controller.Update(updated);
			var ok = Assert.IsType<OkObjectResult>(result.Result);
			var prefs = Assert.IsType<UserPreferences>(ok.Value);
			Assert.Equal(24, prefs.PasswordLength);
			Assert.True(prefs.UseSymbols);
			Assert.True(prefs.AutoSaveEntries);
			Assert.Equal(60, prefs.SessionTimeoutMinutes);
		}

		[Fact]
		public void Update_UpdatesExistingPreferences()
		{
			var context = CreateInMemoryContext();
			var userId = Guid.NewGuid();
			var controller = CreateController(context, userId);
			// Create initial
			controller.Update(new UserPreferences { PasswordLength = 12 });
			// Update
			var updated = new UserPreferences { PasswordLength = 32, UseUppercase = false };
			var result = controller.Update(updated);
			var ok = Assert.IsType<OkObjectResult>(result.Result);
			var prefs = Assert.IsType<UserPreferences>(ok.Value);
			Assert.Equal(32, prefs.PasswordLength);
			Assert.False(prefs.UseUppercase);
		}

		[Fact]
		public void Get_Unauthorized_WhenNoUser()
		{
			var context = CreateInMemoryContext();
			var service = new PreferencesService(context);
            var controller = new PreferencesController(service)
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
            };
            var result = controller.Get();
			Assert.IsType<UnauthorizedResult>(result.Result);
		}

		[Fact]
		public void Update_Unauthorized_WhenNoUser()
		{
			var context = CreateInMemoryContext();
			var service = new PreferencesService(context);
			var controller = new PreferencesController(service);
			controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
			var updated = new UserPreferences();
			var result = controller.Update(updated);
			Assert.IsType<UnauthorizedResult>(result.Result);
		}

		[Fact]
		public void GeneratePassword_UsesPreferencesDefaults()
		{
			var context = CreateInMemoryContext();
			var userId = Guid.NewGuid();
			var controller = CreateController(context, userId);
			// Set preferences
			controller.Update(new UserPreferences { PasswordLength = 22, UseUppercase = true, UseLowercase = true, UseNumbers = true, UseSymbols = false });
			var result = controller.GeneratePassword(null, null, null, null, null);
			var ok = Assert.IsType<OkObjectResult>(result.Result);
			var passwordProp = ok.Value?.GetType().GetProperty("password");
			string? password = passwordProp?.GetValue(ok.Value, null) as string;
			Assert.NotNull(password);
			Assert.Equal(22, password!.Length);
		}

		[Fact]
		public void GeneratePassword_OverridesPreferences()
		{
			var context = CreateInMemoryContext();
			var userId = Guid.NewGuid();
			var controller = CreateController(context, userId);
			controller.Update(new UserPreferences { PasswordLength = 10, UseUppercase = false, UseLowercase = true, UseNumbers = false, UseSymbols = false });
			var result = controller.GeneratePassword(30, true, false, true, false);
			var ok = Assert.IsType<OkObjectResult>(result.Result);
			var passwordProp = ok.Value?.GetType().GetProperty("password");
			string? password = passwordProp?.GetValue(ok.Value, null) as string;
			Assert.NotNull(password);
			Assert.Equal(30, password!.Length);
			Assert.Equal(30, password.Length);
		}
	}
}
