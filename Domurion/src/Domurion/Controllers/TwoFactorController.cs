using Domurion.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Domurion.Services;
using Domurion.Data;
using System.Security.Claims;
using OtpNet;
using QRCoder;

namespace Domurion.Controllers
{
    [ApiController]
    [Route("api/2fa")]
    [Authorize]
    public class TwoFactorController(UserService userService, DataContext context) : ControllerBase
    {
        private readonly UserService _userService = userService;
        private readonly DataContext _context = context;

        [HttpPost("enable")]
        public IActionResult Enable2FA()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var user = _context.Users.FirstOrDefault(u => u.Id == Guid.Parse(userId));
            if (user == null) return NotFound();
            if (user.TwoFactorEnabled) return BadRequest("2FA already enabled.");

            // Generate secret
            var secret = KeyGeneration.GenerateRandomKey(20);
            var base32Secret = Base32Encoding.ToString(secret);
            user.TwoFactorSecret = base32Secret;
            _context.SaveChanges();
            // Audit log
            AuditLogger.Log(_context, user.Id, user.Username, null, "Enable2FA", null);

            // Generate QR code (otpauth://)
            var issuer = "Domurion";
            var label = user.Username;
            var otpauth = $"otpauth://totp/{issuer}:{label}?secret={base32Secret}&issuer={issuer}";
            using var qrGenerator = new QRCodeGenerator();
            using var qrData = qrGenerator.CreateQrCode(otpauth, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrData);
            var qrCodeBytes = qrCode.GetGraphic(20);
            var qrCodeBase64 = Convert.ToBase64String(qrCodeBytes);

            return Ok(new { secret = base32Secret, qrCode = $"data:image/png;base64,{qrCodeBase64}" });
        }

        [HttpPost("verify")]
        public IActionResult Verify2FA([FromBody] string code)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var user = _context.Users.FirstOrDefault(u => u.Id == Guid.Parse(userId));
            if (user == null || string.IsNullOrEmpty(user.TwoFactorSecret)) return BadRequest("2FA not setup.");

            var totp = new Totp(Base32Encoding.ToBytes(user.TwoFactorSecret));
            if (!totp.VerifyTotp(code, out _, new VerificationWindow(previous: 1, future: 1)))
            {
                // Audit log
                AuditLogger.Log(_context, user.Id, user.Username, null, "2FAVerifyFailed", null);
                return BadRequest("Invalid code.");
            }

            user.TwoFactorEnabled = true;
            _context.SaveChanges();
            // Audit log
            AuditLogger.Log(_context, user.Id, user.Username, null, "2FAVerifySuccess", null);
            return Ok("2FA enabled successfully.");
        }

        [HttpPost("disable")]
        public IActionResult Disable2FA([FromBody] string code)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var user = _context.Users.FirstOrDefault(u => u.Id == Guid.Parse(userId));
            if (user == null || !user.TwoFactorEnabled || string.IsNullOrEmpty(user.TwoFactorSecret))
                return BadRequest("2FA not enabled.");

            var totp = new Totp(Base32Encoding.ToBytes(user.TwoFactorSecret));
            if (!totp.VerifyTotp(code, out _, new VerificationWindow(previous: 1, future: 1)))
            {
                // Audit log
                AuditLogger.Log(_context, user.Id, user.Username, null, "2FADisableFailed", null);
                return BadRequest("Invalid code.");
            }

            user.TwoFactorEnabled = false;
            user.TwoFactorSecret = null;
            user.TwoFactorRecoveryCodes = null;
            _context.SaveChanges();
            // Audit log
            AuditLogger.Log(_context, user.Id, user.Username, null, "Disable2FA", null);
            return Ok("2FA disabled.");
        }

        [HttpPost("generate-recovery-codes")]
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
        public IActionResult Get2FAStatus()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var user = _context.Users.FirstOrDefault(u => u.Id == Guid.Parse(userId));
            if (user == null) return NotFound();
            return Ok(new { enabled = user.TwoFactorEnabled });
        }

        [HttpGet("recovery-codes")]
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
