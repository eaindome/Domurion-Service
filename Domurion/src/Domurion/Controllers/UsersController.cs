using Domurion.Dtos;
using Domurion.Helpers;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Domurion.Services.Interfaces;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Authorization;
using Domurion.Services;

namespace Domurion.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableRateLimiting("fixed")]
    public class UsersController(IUserService userService, PreferencesService preferencesService, Domurion.Helpers.EmailService emailService) : ControllerBase
    {
        private readonly IUserService _userService = userService;
        private readonly PreferencesService _preferencesService = preferencesService;
        private readonly Domurion.Helpers.EmailService _emailService = emailService;

        [HttpPost("register")]
        public IActionResult Register([FromBody] UserDto userDto)
        {
            try
            {
                var username = userDto.Email.Split('@')[0];
                var user = _userService.Register(userDto.Email, userDto.Password, userDto.Name, username);
                var verificationUrl = $"https://domurion-service.vercel.app/verify/email?email={userDto.Email}&token={user.EmailVerificationToken}";
                var subject = "Verify your email address";
                var body = $"Welcome! Please verify your email by clicking this link: {verificationUrl}";
                _emailService.SendEmail(user.Email, subject, body);
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

        [HttpPost("login")]
        public IActionResult Login([FromBody] UserDto userDto)
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
                    // Expect 2FA code in X-2FA-Code header
                    var code = Request.Headers["X-2FA-Code"].ToString();
                    if (string.IsNullOrWhiteSpace(code))
                        return Unauthorized(new { error = "2FA code required.", twoFactorRequired = true });

                    bool valid2FA = false;
                    // Check TOTP
                    if (!string.IsNullOrEmpty(user.TwoFactorSecret))
                    {
                        var totp = new OtpNet.Totp(OtpNet.Base32Encoding.ToBytes(user.TwoFactorSecret));
                        if (totp.VerifyTotp(code, out _, new OtpNet.VerificationWindow(previous: 1, future: 1)))
                            valid2FA = true;
                    }
                    // Check recovery codes if TOTP failed
                    if (!valid2FA && !string.IsNullOrEmpty(user.TwoFactorRecoveryCodes))
                    {
                        var codes = user.TwoFactorRecoveryCodes.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
                        if (codes.Contains(code))
                        {
                            valid2FA = true;
                            // Remove used code
                            codes.Remove(code);
                            user.TwoFactorRecoveryCodes = string.Join(",", codes);
                            // Save change to DB using a new DataContext instance
                            using var scope = HttpContext.RequestServices.CreateScope();
                            if (scope.ServiceProvider.GetService(typeof(Domurion.Data.DataContext)) is Domurion.Data.DataContext db)
                            {
                                var dbUser = db.Users.FirstOrDefault(u => u.Id == user.Id);
                                if (dbUser != null)
                                {
                                    dbUser.TwoFactorRecoveryCodes = user.TwoFactorRecoveryCodes;
                                    db.SaveChanges();
                                }
                            }
                        }
                    }
                    if (!valid2FA)
                        return Unauthorized(new { error = "Invalid 2FA or recovery code.", twoFactorRequired = true });
                }

                // Fetch user preferences for session timeout
                var prefs = _preferencesService.GetPreferences(user.Id);
                // Generate JWT token with session timeout
                var token = Helpers.JwtHelper.GenerateJwtToken(user, HttpContext.RequestServices.GetService<IConfiguration>()!, prefs);

                // Send login notification email
                try
                {
                    var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                    var time = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss 'UTC'");
                    var subject = "New Login to Your Account";
                    var body = $"A new login to your account was detected.\n\nUsername: {user.Username}\nTime: {time}\nIP Address: {ip}\nIf this was not you, please reset your password immediately.";
                    _emailService.SendEmail(user.Username, subject, body);
                }
                catch { /* Ignore email errors for login */ }

                return Ok(new { user.Id, user.Username, token });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
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

        [HttpGet("verify-email")]
        [AllowAnonymous]
        public IActionResult VerifyEmail([FromQuery] string email, [FromQuery] string token)
        {
            var user = _userService.GetByVerificationToken(token);
            if (user == null || !string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase))
                return BadRequest("Invalid or expired verification token.");

            user.EmailVerified = true;
            user.EmailVerificationToken = null;
            _userService.Save(user);

            return Ok("Email verified successfully. You can now log in.");
        }



        [HttpGet("generate-password")]
        [Authorize]
        public IActionResult GeneratePassword([FromQuery] int length = 16)
        {
            var password = Helper.GeneratePassword(length);
            return Ok(new { password });
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
    }
}