using Microsoft.AspNetCore.Mvc;
using Domurion.Data;
using Domurion.Models;
using Domurion.Helpers;

namespace Domurion.Controllers
{
    [ApiController]
    [Route("api/support")]
    public class SupportController(DataContext context) : ControllerBase
    {
        private readonly DataContext _context = context;

        [HttpPost("request-2fa-reset")]
        public IActionResult Request2FAReset([FromBody] SupportRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) && string.IsNullOrWhiteSpace(request.Email))
                return BadRequest("Username or email is required.");
            request.Id = Guid.NewGuid();
            request.RequestedAt = DateTime.UtcNow;
            request.Resolved = false;
            _context.SupportRequests.Add(request);
            _context.SaveChanges();
            return Ok("Your request has been submitted. The admin will review it soon.");
        }

        // List unresolved support requests
        [HttpGet("requests")]
        public IActionResult GetRequests()
        {
            var requests = _context.SupportRequests.Where(r => !r.Resolved).OrderBy(r => r.RequestedAt).ToList();
            return Ok(requests);
        }

        // Resolve a support request and reset 2FA for the user
        [HttpPost("resolve-2fa-reset/{requestId}")]
        public IActionResult Resolve2FAReset(Guid requestId, [FromBody] string resolutionNote)
        {
            var request = _context.SupportRequests.FirstOrDefault(r => r.Id == requestId);
            if (request == null || request.Resolved)
                return NotFound("Request not found or already resolved.");

            // Find user by username or email
            var user = _context.Users.FirstOrDefault(u =>
                (request.UserId != null && u.Id == request.UserId) ||
                (!string.IsNullOrEmpty(request.Username) && u.Username == request.Username) ||
                (!string.IsNullOrEmpty(request.Email) && u.Username == request.Email)
            );
            if (user == null)
                return NotFound("User not found.");

            // Reset 2FA
            user.TwoFactorEnabled = false;
            user.TwoFactorSecret = null;
            user.TwoFactorRecoveryCodes = null;
            request.Resolved = true;
            request.ResolvedAt = DateTime.UtcNow;
            request.ResolutionNote = resolutionNote;
            _context.SaveChanges();
            // Audit log
            AuditLogger.Log(_context, user.Id, user.Username, null, "Admin2FAReset", null);
            return Ok("2FA has been reset and request marked as resolved.");
        }
    }
}
