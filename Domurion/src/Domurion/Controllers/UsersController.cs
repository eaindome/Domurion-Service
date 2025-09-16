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
                var verificationUrl = $"https://domurion-service.vercel.app/verify/{user.EmailVerificationToken}";
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
                    // genererrate otp
                    var otp = new Random().Next(100000, 999999).ToString();
                    user.PendingOtp = otp;
                    user.PendingOtpExpiresAt = DateTime.UtcNow.AddMinutes(5);
                    _userService.Save(user);

                    // send OTP via email
                    var subject = "Your login OTP";
                    var body = $"Your one-time password (OTP) is: {otp}\nIt expires in 5 minutes.";
                    _emailService.SendEmail(user.Email, subject, body);

                    return Unauthorized(new { error = "OTP required.", twoFactorRequired = true });
                }

                // Fetch user preferences for session timeout
                var prefs = _preferencesService.GetPreferences(user.Id);
                // Generate JWT token with session timeout
                var token = JwtHelper.GenerateJwtToken(user, HttpContext.RequestServices.GetService<IConfiguration>()!, prefs);

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

            return Ok(new { user.Id, user.Username, token });
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
            var body = $"Please verify your email by clicking this link: {verificationUrl}";
            _emailService.SendEmail(user.Email, subject, body);

            return Ok(new { message = "Verification email sent." });
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