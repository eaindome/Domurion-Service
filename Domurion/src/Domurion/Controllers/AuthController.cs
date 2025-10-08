using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Domurion.Helpers;
using Domurion.Services.Interfaces;
using Domurion.Models;
using Domurion.Services;
using System.Security.Claims;

namespace Domurion.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController(IUserService userService, IConfiguration configuration, IPreferencesService preferencesService) : ControllerBase
    {
        private readonly IUserService _userService = userService;
        private readonly IConfiguration _configuration = configuration;
        private readonly IPreferencesService _preferencesService = preferencesService;

        [HttpGet("google-login")]
        public IActionResult GoogleLogin(string? returnUrl = null)
        {
            var redirectUrl = Url.Action("GoogleResponse", "Auth", new { returnUrl });
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("signin-google")]
        [AllowAnonymous]
        public async Task<IActionResult> GoogleResponse(string? returnUrl = null)
        {
            var authenticateResult = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
            if (!authenticateResult.Succeeded || authenticateResult.Principal == null)
                return Unauthorized(new { message = "Google authentication failed." });

            var email = authenticateResult.Principal.FindFirst(ClaimTypes.Email)?.Value;
            var name = authenticateResult.Principal.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(email))
                return BadRequest(new { message = "Google account email not found." });

            // Find or create user in your DB
            var user = _userService.GetByUsername(email);
            user ??= _userService.CreateExternalUser(email, name, "Google");

            // Fetch user preferences for session timeout
            var prefs = _preferencesService.GetPreferences(user.Id);
            // Generate JWT for your app with session timeout
            var token = JwtHelper.GenerateJwtToken(user, _configuration, prefs);

            // Send login notification email
            try
            {
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                var time = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss 'UTC'");
                var userAgent = Request.Headers.UserAgent.ToString();
                string browser = "Unknown";
                string os = "Unknown";
                if (!string.IsNullOrEmpty(userAgent))
                {
                    if (userAgent.Contains("Windows")) os = "Windows";
                    else if (userAgent.Contains("Mac")) os = "MacOS";
                    else if (userAgent.Contains("Linux")) os = "Linux";
                    else if (userAgent.Contains("Android")) os = "Android";
                    else if (userAgent.Contains("iPhone") || userAgent.Contains("iPad")) os = "iOS";

                    if (userAgent.Contains("Chrome")) browser = "Chrome";
                    else if (userAgent.Contains("Firefox")) browser = "Firefox";
                    else if (userAgent.Contains("Safari") && !userAgent.Contains("Chrome")) browser = "Safari";
                    else if (userAgent.Contains("Edge")) browser = "Edge";
                    else if (userAgent.Contains("MSIE") || userAgent.Contains("Trident")) browser = "Internet Explorer";
                }
                // Location info (geo-IP)
                string city = "Unknown";
                string country = "Unknown";
                try
                {
                    using var httpClient = new System.Net.Http.HttpClient();
                    var response = await httpClient.GetStringAsync($"http://ip-api.com/json/{ip}");
                    var geo = System.Text.Json.JsonDocument.Parse(response);
                    if (geo.RootElement.GetProperty("status").GetString() == "success")
                    {
                        city = geo.RootElement.GetProperty("city").GetString() ?? "Unknown";
                        country = geo.RootElement.GetProperty("country").GetString() ?? "Unknown";
                    }
                }
                catch { /* Ignore geo-IP errors */ }
                // URLs
                var secureAccountUrl = "https://domurion-service.vercel.app/reset-password";
                var dashboardUrl = "https://domurion-service.vercel.app/dashboard";
                var placeholders = new Dictionary<string, string>
                {
                    { "USER_EMAIL", user.Email },
                    { "LOGIN_TIME", time },
                    { "IP_ADDRESS", ip },
                    { "BROWSER", browser },
                    { "OPERATING_SYSTEM", os },
                    { "LOCATION_CITY", city },
                    { "LOCATION_COUNTRY", country },
                    { "SECURE_ACCOUNT_URL", secureAccountUrl },
                    { "DASHBOARD_URL", dashboardUrl }
                };
                var templatePath = "Templates/Email/login_notification.html";
                var emailService = HttpContext.RequestServices.GetService(typeof(Domurion.Helpers.EmailService)) as Domurion.Helpers.EmailService;
                var subject = "New Login to Your Account";
                var body = emailService?.RenderTemplate(templatePath, placeholders) ?? string.Empty;
                emailService?.SendEmail(user.Email, subject, body, isHtml: true);
            }
            catch { /* Ignore email errors for login */ }

            // Optionally, redirect to frontend with token as query param
            if (!string.IsNullOrEmpty(returnUrl))
                return Redirect(returnUrl + "?token=" + token);

            return Ok(new { token });
        }
    }
}
