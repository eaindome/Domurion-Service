using Domurion.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Domurion.Services;
using Domurion.Data;
using Domurion.Dtos;
using System.Security.Claims;
using OtpNet;

namespace Domurion.Controllers
{
    [ApiController]
    [Route("api/2fa")]
    [Authorize]
    public class TwoFactorController(UserService userService, PreferencesService preferencesService, DataContext context) : ControllerBase
    {
        private readonly UserService _userService = userService;
        private readonly PreferencesService _preferencesService = preferencesService;
        private readonly DataContext _context = context;

        [HttpPost("enable")]
        [Authorize]
        public IActionResult Enable2FA()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var user = _context.Users.FirstOrDefault(u => u.Id == Guid.Parse(userId));
            if (user == null) return NotFound();
            if (user.TwoFactorEnabled) return BadRequest("2FA already enabled.");

            user.TwoFactorEnabled = true;
            _context.SaveChanges();
            AuditLogger.Log(_context, user.Id, user.Username, null, "Enable2FA", null);
            return Ok("2FA enabled. You will receive OTPs via email when logging in.");
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

        [HttpPost("disable")]
        [Authorize]
        public IActionResult Disable2FA()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var user = _context.Users.FirstOrDefault(u => u.Id == Guid.Parse(userId));
            if (user == null) return NotFound();
            if (!user.TwoFactorEnabled) return BadRequest("2FA already disabled.");

            user.TwoFactorEnabled = false;
            _context.SaveChanges();
            AuditLogger.Log(_context, user.Id, user.Username, null, "Disable2FA", null);
            return Ok("2FA disabled.");
        }


        [HttpPost("generate-recovery-codes")]
        [Authorize]
        public IActionResult GenerateRecoveryCodes()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var user = _context.Users.FirstOrDefault(u => u.Id == Guid.Parse(userId));
            if (user == null || !user.TwoFactorEnabled) return BadRequest("2FA not enabled.");

            // Generate 10 random recovery codes
            var codes = new List<string>();
            for (int i = 0; i < 10; i++)
            {
                codes.Add(Guid.NewGuid().ToString("N").Substring(0, 8));
            }
            user.TwoFactorRecoveryCodes = string.Join(",", codes);
            _context.SaveChanges();
            // Audit log
            AuditLogger.Log(_context, user.Id, user.Username, null, "Generate2FARecoveryCodes", null);
            return Ok(new { recoveryCodes = codes });
        }

        [HttpPost("use-recovery-code")]
        [Authorize]
        public IActionResult UseRecoveryCode([FromBody] string code)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var user = _context.Users.FirstOrDefault(u => u.Id == Guid.Parse(userId));
            if (user == null || !user.TwoFactorEnabled || string.IsNullOrEmpty(user.TwoFactorRecoveryCodes))
                return BadRequest("No recovery codes available.");

            var codes = user.TwoFactorRecoveryCodes.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
            if (!codes.Contains(code))
            {
                // Audit log
                AuditLogger.Log(_context, user.Id, user.Username, null, "2FARecoveryCodeFailed", null);
                return BadRequest("Invalid recovery code.");
            }

            // Remove used code
            codes.Remove(code);
            user.TwoFactorRecoveryCodes = string.Join(",", codes);
            _context.SaveChanges();
            // Audit log
            AuditLogger.Log(_context, user.Id, user.Username, null, "2FARecoveryCodeUsed", null);
            return Ok("Recovery code accepted.");
        }

        [HttpGet("status")]
        [Authorize]
        public IActionResult Get2FAStatus()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var user = _context.Users.FirstOrDefault(u => u.Id == Guid.Parse(userId));
            if (user == null) return NotFound();
            return Ok(new { enabled = user.TwoFactorEnabled });
        }

        [HttpGet("recovery-codes")]
        [Authorize]
        public IActionResult GetRecoveryCodes()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var user = _context.Users.FirstOrDefault(u => u.Id == Guid.Parse(userId));
            if (user == null || !user.TwoFactorEnabled || string.IsNullOrEmpty(user.TwoFactorRecoveryCodes))
                return BadRequest("No recovery codes available.");
            // Only show codes if user is authenticated and 2FA is enabled
            var codes = user.TwoFactorRecoveryCodes.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
            return Ok(new { recoveryCodes = codes });
        }

        [HttpPost("regenerate-recovery-codes")]
        [Authorize]
        public IActionResult RegenerateRecoveryCodes([FromBody] string code)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var user = _context.Users.FirstOrDefault(u => u.Id == Guid.Parse(userId));
            if (user == null || !user.TwoFactorEnabled || string.IsNullOrEmpty(user.TwoFactorSecret))
                return BadRequest("2FA not enabled.");
            // Require valid TOTP code to regenerate
            var totp = new Totp(Base32Encoding.ToBytes(user.TwoFactorSecret));
            if (!totp.VerifyTotp(code, out _, new VerificationWindow(previous: 1, future: 1)))
                return BadRequest("Invalid code.");
            // Generate new codes
            var codes = new List<string>();
            for (int i = 0; i < 10; i++)
            {
                codes.Add(Guid.NewGuid().ToString("N").Substring(0, 8));
            }
            user.TwoFactorRecoveryCodes = string.Join(",", codes);
            _context.SaveChanges();
            // Audit log
            AuditLogger.Log(_context, user.Id, user.Username, null, "Regenerate2FARecoveryCodes", null);
            return Ok(new { recoveryCodes = codes });
        }
    }
}
