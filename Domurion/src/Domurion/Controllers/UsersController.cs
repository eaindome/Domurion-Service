using Domurion.Dtos;
using Domurion.Helpers;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Domurion.Services.Interfaces;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Authorization;
using Domurion.Services;
using System.Text.Json;

namespace Domurion.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableRateLimiting("fixed")]
    public class UsersController(IUserService userService, PreferencesService preferencesService, PasswordVaultService passwordVaultService, EmailService emailService) : ControllerBase
    {
        private readonly IUserService _userService = userService;
        private readonly PreferencesService _preferencesService = preferencesService;
        private readonly PasswordVaultService _passwordVaultService = passwordVaultService;
        private readonly EmailService _emailService = emailService;

        #region User Management
        [HttpPost("register")]
        public IActionResult Register([FromBody] UserDto userDto)
        {
            try
            {
                var username = userDto.Email.Split('@')[0];
                var user = _userService.Register(userDto.Email, userDto.Password, userDto.Name, username);
                var verificationUrl = $"https://domurion-service.vercel.app/verify/{user.EmailVerificationToken}";
                var subject = "Verify your email address";
                // Render verification template
                var placeholders = new Dictionary<string, string>
                {
                    { "USER_EMAIL", user.Email },
                    { "VERIFICATION_URL", verificationUrl }
                };
                var templatePath = "Templates/Email/verification.html";
                var body = _emailService.RenderTemplate(templatePath, placeholders);
                _emailService.SendEmail(user.Email, subject, body, isHtml: true);
                return Ok(new { user.Id, user.Username, user.Name });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { error = ex.Message });
            }
        }

        [HttpGet("me")]
        [Authorize]
        public IActionResult GetCurrentUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var user = _userService.GetById(Guid.Parse(userId));
            if (user == null)
                return NotFound();

            return Ok(new
            {
                user.Id,
                user.Username,
                user.Name,
                user.AuthProvider,
                user.GoogleId,
                user.TwoFactorEnabled
            });
        }

        [HttpPut("update")]
        [Authorize]
        public IActionResult Update(Guid userId, string? newUsername, string? newPassword)
        {
            try
            {
                // Accept name as an optional query parameter for update
                var name = Request.Query["name"].ToString();
                if (string.IsNullOrWhiteSpace(name)) name = null;
                var user = _userService.UpdateUser(userId, newUsername, newPassword, name);
                return Ok(new { user.Id, user.Username, user.Name });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { error = ex.Message });
            }
        }

        [HttpDelete("delete")]
        [Authorize]
        public IActionResult Delete(Guid userId)
        {
            try
            {
                _userService.DeleteUser(userId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }
        #endregion

        #region Authentications
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserDto userDto)
        {
            try
            {
                var user = _userService.Login(userDto.Email, userDto.Password);
                if (user == null)
                    return Unauthorized(new { error = "Invalid email or password." });

                if (!user.EmailVerified)
                    return Unauthorized(new { error = "Please verify your email before logging in." });
                // If 2FA is enabled, require 2FA code
                if (user.TwoFactorEnabled)
                {
                    // genererrate otp
                    var otp = new Random().Next(100000, 999999).ToString();
                    user.PendingOtp = otp;
                    user.PendingOtpExpiresAt = DateTime.UtcNow.AddMinutes(10);
                    _userService.Save(user);

                    // send OTP via email using template
                    var subject = "Your login OTP";
                    var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                    var time = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss 'UTC'");
                    var userAgent = Request.Headers.UserAgent.ToString();
                    string location = "Unknown";
                    try
                    {
                        using var httpClient = new HttpClient();
                        var response = httpClient.GetStringAsync($"http://ip-api.com/json/{ip}").Result;
                        var geo = JsonDocument.Parse(response);
                        if (geo.RootElement.GetProperty("status").GetString() == "success")
                        {
                            var city = geo.RootElement.GetProperty("city").GetString() ?? "Unknown";
                            var country = geo.RootElement.GetProperty("country").GetString() ?? "Unknown";
                            location = $"{city}, {country}";
                        }
                    }
                    catch { /* Ignore geo-IP errors */ }
                    var placeholders = new Dictionary<string, string>
                    {
                        { "USER_EMAIL", user.Email },
                        { "OTP_CODE", otp },
                        { "LOGIN_TIME", time },
                        { "IP_ADDRESS", ip },
                        { "LOCATION", location },
                        { "USER_AGENT", userAgent }
                    };
                    var templatePath = "Templates/Email/login_otp.html";
                    var body = _emailService.RenderTemplate(templatePath, placeholders);
                    _emailService.SendEmail(user.Email, subject, body, isHtml: true);

                    return Unauthorized(new { error = "OTP required.", twoFactorRequired = true });
                }

                // Fetch user preferences for session timeout
                var prefs = _preferencesService.GetPreferences(user.Id);
                // Generate JWT token with session timeout
                var token = JwtHelper.GenerateJwtToken(user, HttpContext.RequestServices.GetService<IConfiguration>()!, prefs);

                // Send login notification email
                await SendLoginNotificationEmailAsync(user, "New Login to Your Account");

                return Ok(new { user.Id, user.Username, token });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("verify-email")]
        [AllowAnonymous]
        public IActionResult VerifyEmail([FromQuery] string email, [FromQuery] string token)
        {
            var user = _userService.GetByVerificationToken(token);
            if (user == null
                || user.EmailVerificationTokenExpiresAt == null
                || user.EmailVerificationTokenExpiresAt < DateTime.UtcNow)
                return BadRequest("Invalid or expired verification token.");

            user.EmailVerified = true;
            user.EmailVerificationToken = null;
            user.EmailVerificationTokenExpiresAt = null;
            _userService.Save(user);

            return Ok("Email verified successfully. You can now log in.");
        }


        [HttpPost("resend-verification")]
        [AllowAnonymous]
        public IActionResult ResendVerification([FromBody] string email)
        {
            var user = _userService.GetByEmail(email);
            if (user == null)
                return NotFound(new { error = "User not found." });

            if (user.LastVerificationEmailSentAt != null && user.LastVerificationEmailSentAt > DateTime.UtcNow.AddMinutes(-1))
                return BadRequest(new { error = "Please wait before requesting another verification email." });

            if (user.EmailVerified)
                return BadRequest(new { error = "Email is already verified." });

            // Generate a new token if missing or expired (optional: always generate new)
            user.EmailVerificationToken = Guid.NewGuid().ToString("N");
            user.EmailVerificationTokenExpiresAt = DateTime.UtcNow.AddHours(24);
            user.LastVerificationEmailSentAt = DateTime.UtcNow;
            _userService.Save(user);

            var verificationUrl = $"https://domurion-service.vercel.app/verify/{user.EmailVerificationToken}";
            var subject = "Verify your email address";
            // Render verification template
            var placeholders = new Dictionary<string, string>
            {
                { "USER_EMAIL", user.Email },
                { "VERIFICATION_URL", verificationUrl }
            };
            var templatePath = "Templates/Email/verification.html";
            var body = _emailService.RenderTemplate(templatePath, placeholders);
            _emailService.SendEmail(user.Email, subject, body);

            return Ok(new { message = "Verification email sent." });
        }
        #endregion

        #region Otp management
        [HttpPost("verify-otp")]
        [AllowAnonymous]
        public IActionResult VerifyOtp([FromBody] OtpDto dto)
        {
            var user = _userService.GetByEmail(dto.Email);
            if (user == null || !user.TwoFactorEnabled)
                return Unauthorized(new { error = "Invalid user or 2FA not enabled." });

            if (string.IsNullOrEmpty(user.PendingOtp) ||
                user.PendingOtpExpiresAt == null ||
                user.PendingOtpExpiresAt < DateTime.UtcNow ||
                user.PendingOtp != dto.Otp)
            {
                return Unauthorized(new { error = "Invalid or expired OTP." });
            }

            // Clear OTP after use
            user.PendingOtp = null;
            user.PendingOtpExpiresAt = null;
            _userService.Save(user);

            // Generate JWT and return as in normal login
            var prefs = _preferencesService.GetPreferences(user.Id);
            var token = JwtHelper.GenerateJwtToken(user, HttpContext.RequestServices.GetService<IConfiguration>()!, prefs);
            // Send login notification email (reuse helper)
            SendLoginNotificationEmailAsync(user, "New Login to Your Account").ConfigureAwait(false);
            return Ok(new { user.Id, user.Username, token });
        }

        [HttpPost("request-view-otp")]
        [Authorize]
        public IActionResult RequestViewOtp(Guid credentialId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var user = _userService.GetById(Guid.Parse(userId));
            if (user == null || !user.TwoFactorEnabled) return BadRequest("2FA not enabled.");

            var otp = new Random().Next(100000, 999999).ToString();
            user.PendingOtp = otp;
            user.PendingOtpExpiresAt = DateTime.UtcNow.AddMinutes(5);
            _userService.Save(user);

            var subject = "Your OTP for Viewing Password";
            // Gather info for template
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var time = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss 'UTC'");
            var userAgent = Request.Headers.UserAgent.ToString();
            string location = "Unknown";
            try
            {
                using var httpClient = new HttpClient();
                var response = httpClient.GetStringAsync($"http://ip-api.com/json/{ip}").Result;
                var geo = JsonDocument.Parse(response);
                if (geo.RootElement.GetProperty("status").GetString() == "success")
                {
                    var city = geo.RootElement.GetProperty("city").GetString() ?? "Unknown";
                    var country = geo.RootElement.GetProperty("country").GetString() ?? "Unknown";
                    location = $"{city}, {country}";
                }
            }
            catch { /* Ignore geo-IP errors */ }

            var credential = _passwordVaultService.GetById(credentialId);
            string credentialSite = credential?.Site ?? "Vault";
            string credentialUsername = user.Username;
            var placeholders = new Dictionary<string, string>
            {
                { "USER_EMAIL", user.Email },
                { "CREDENTIAL_SITE", credentialSite },
                { "CREDENTIAL_USERNAME", credentialUsername },
                { "OTP_CODE", otp },
                { "REQUEST_TIME", time },
                { "IP_ADDRESS", ip },
                { "LOCATION", location },
                { "USER_AGENT", userAgent }
            };
            var templatePath = "Templates/Email/view_otp.html";
            var body = _emailService.RenderTemplate(templatePath, placeholders);
            _emailService.SendEmail(user.Email, subject, body, isHtml: true);

            return Ok(new { message = "OTP sent to your email." });
        }

        [HttpPost("verify-view-otp")]
        [Authorize]
        public IActionResult VerifyViewOtp([FromBody] OtpDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var user = _userService.GetById(Guid.Parse(userId));
            if (user == null || !user.TwoFactorEnabled) return BadRequest("2FA not enabled.");

            if (string.IsNullOrEmpty(user.PendingOtp) ||
                user.PendingOtpExpiresAt == null ||
                user.PendingOtpExpiresAt < DateTime.UtcNow ||
                user.PendingOtp != dto.Otp)
            {
                return Unauthorized(new { error = "Invalid or expired OTP." });
            }

            // Clear OTP after use
            user.PendingOtp = null;
            user.PendingOtpExpiresAt = null;
            _userService.Save(user);

            return Ok(new { verified = true });
        }
        #endregion

        #region Password generation
        [HttpGet("generate-password")]
        [Authorize]
        public IActionResult GeneratePassword([FromQuery] int length = 16)
        {
            var password = Helper.GeneratePassword(length);
            return Ok(new { password });
        }
        #endregion

        #region Linking accounts
        [HttpPost("link-google")]
        [Authorize]
        public IActionResult LinkGoogle([FromBody] string googleId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            var result = _userService.LinkGoogleAccount(Guid.Parse(userId), googleId);
            if (!result)
                return BadRequest("Google account already linked to another user.");
            return Ok("Google account linked successfully.");
        }

        [HttpPost("unlink-google")]
        [Authorize]
        public IActionResult UnlinkGoogle()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            var result = _userService.UnlinkGoogleAccount(Guid.Parse(userId));
            if (!result)
                return BadRequest("No Google account to unlink.");
            return Ok("Google account unlinked successfully.");
        }
        #endregion

        #region Helpers
        // Private helper for login notification email
        private async Task SendLoginNotificationEmailAsync(Domurion.Models.User user, string subject)
        {
            try
            {
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                var time = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss 'UTC'");
                // Get browser and OS info from headers (best effort)
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
                    using var httpClient = new HttpClient();
                    var response = await httpClient.GetStringAsync($"http://ip-api.com/json/{ip}");
                    var geo = JsonDocument.Parse(response);
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
                var body = _emailService.RenderTemplate(templatePath, placeholders);
                _emailService.SendEmail(user.Email, subject, body, isHtml: true);
            }
            catch { /* Ignore email errors for login */ }
        }
        #endregion
    }
}