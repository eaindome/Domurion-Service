using Domurion.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Domurion.Dtos;
using Domurion.Data;
using System.Security.Claims;
using Domurion.Models;

namespace Domurion.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VaultController(IPasswordVaultService passwordVaultService, IUserService userService, DataContext context) : ControllerBase
    {
        private readonly IPasswordVaultService _passwordVaultService = passwordVaultService;
        private readonly IUserService _userService = userService;
        private readonly DataContext _context = context;

        [HttpPost("add")]
        [Authorize]
        public IActionResult Add(string site, string email, string password, string? notes = null, string? siteUrl = null)
        {
            var validationResult = ValidateAndGetUser(out var user);
            if (validationResult != null) return validationResult;

            try
            {
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
                var credential = _passwordVaultService.AddCredential(user!.Id, site, siteUrl, email, password, notes, ip);
                return Ok(new { credential.Id, credential.Site, credential.Email, credential.SiteUrl });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("list")]
        [Authorize]
        public IActionResult List()
        {
            var validationResult = ValidateAndGetUser(out var user);
            if (validationResult != null) return validationResult;

            var credentials = _passwordVaultService.GetCredentials(user!.Id)
                .Select(c => new
                {
                    c.Id,
                    c.Site,
                    c.Email,
                    c.Notes,
                    c.CreatedAt,
                    c.UpdatedAt,
                    Password = _passwordVaultService.RetrievePassword(c.Id, user.Id),
                });
            return Ok(credentials);
        }

        [HttpGet("retrieve")]
        [Authorize]
        public IActionResult Retrieve(Guid credentialId)
        {
            var validationResult = ValidateAndGetUser(out var user);
            if (validationResult != null) return validationResult;

            try
            {
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
                var credential = _passwordVaultService.GetById(credentialId);
                if (credential == null || credential.UserId != user!.Id)
                    return NotFound(new { error = "Credential not found." });

                var password = _passwordVaultService.RetrievePassword(credentialId, user.Id, ip);
                return Ok(new
                {
                    credential.Id,
                    credential.Site,
                    credential.SiteUrl,
                    credential.Email,
                    credential.Notes,
                    Password = password
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        [HttpPut("update")]
        [Authorize]
        public IActionResult Update(Guid credentialId, string? site, string? email, string? password, string? notes = null, string? siteUrl = null)
        {
            var validationResult = ValidateAndGetUser(out var user);
            if (validationResult != null) return validationResult;

            try
            {
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
                var credential = _passwordVaultService.UpdateCredential(credentialId, user!.Id, site, email, password, notes, siteUrl, ip);
                return Ok(new { credential.Id, credential.Site, credential.SiteUrl, credential.Email, credential.Notes });
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
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("delete")]
        [Authorize]
        public IActionResult Delete(Guid credentialId)
        {
            var validationResult = ValidateAndGetUser(out var user);
            if (validationResult != null) return validationResult;
            try
            {
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
                _passwordVaultService.DeleteCredential(credentialId, user!.Id, ip);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        [HttpPost("share")]
        [Authorize]
        public IActionResult Share(Guid credentialId, Guid fromUserId, string toUsername)
        {
            try
            {
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
                var sharedCredential = _passwordVaultService.ShareCredential(credentialId, fromUserId, toUsername, ip);
                return Ok(new { sharedCredential.Id, sharedCredential.Site, sharedCredential.Username });
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
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("export")]
        [Authorize]
        public IActionResult Export()
        {
            var validationResult = ValidateAndGetUser(out var user);
            if (validationResult != null) return validationResult;
            var credentials = _passwordVaultService.GetCredentials(user!.Id)
                .Select(c => new
                {
                    c.Site,
                    c.SiteUrl,
                    c.Username,
                    c.Email,
                    Password = _passwordVaultService.RetrievePassword(c.Id, user.Id),
                    c.Notes,
                    c.CreatedAt,
                    c.UpdatedAt
                });
            return Ok(credentials);
        }

        #region Additional Features
        [HttpPost("import")]
        [Authorize]
        public IActionResult Import([FromBody] List<ImportCredentialDto> credentials)
        {
            var validationResult = ValidateAndGetUser(out var user);
            if (validationResult != null) return validationResult;
            var imported = new List<object>();
            foreach (var cred in credentials)
            {
                var (isValid, identifier, errors) = ValidateImportRow(cred);
                if (!isValid)
                {
                    imported.Add(new { cred.Site, cred.SiteUrl, cred.Username, cred.Email, errors });
                    continue;
                }

                try
                {
                    var c = _passwordVaultService.AddCredential(user!.Id, cred.Site!, cred.SiteUrl, identifier!, cred.Password!, cred.Notes);
                    imported.Add(new { c.Id, c.Site, c.Username, c.SiteUrl });
                }
                catch (Exception ex)
                {
                    imported.Add(new { cred.Site, cred.SiteUrl, cred.Username, cred.Email, error = ex.Message });
                }
            }
            return Ok(imported);
        }

        private static (bool isValid, string? identifier, List<string> errors) ValidateImportRow(ImportCredentialDto cred)
        {
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(cred.Site))
                errors.Add("Missing site field");

            var identifier = !string.IsNullOrWhiteSpace(cred.Username) ? cred.Username : cred.Email;
            if (string.IsNullOrWhiteSpace(identifier))
                errors.Add("Missing username/email");

            if (string.IsNullOrWhiteSpace(cred.Password))
                errors.Add("Missing password");

            return (errors.Count == 0, identifier, errors);
        }

        [HttpDelete("delete-all")]
        [Authorize]
        public IActionResult DeleteAllVaultData()
        {
            var validationResult = ValidateAndGetUser(out var user);
            if (validationResult != null) return validationResult;

            try
            {
                var deletedCount = _passwordVaultService.DeleteAllUserVaultItems(user!.Id.ToString());

                // Also delete user settings if needed
                _userService.ResetUserSettings(user.Id.ToString());

                return Ok(new
                {
                    message = "All vault data has been deleted",
                    deletedCount = deletedCount
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Failed to delete vault data", details = ex.Message });
            }
        }
        #endregion

        [HttpPost("share/secondary")]
        [Authorize]
        public IActionResult Share(Guid credentialId, string toIdentifier)
        {
            var fromUserId = Guid.Parse(User.FindFirst("sub")?.Value ?? throw new UnauthorizedAccessException("User ID not found in token."));
            try
            {
                var invitation = _passwordVaultService.CreateShareInvitation(credentialId, fromUserId, toIdentifier, _context);
                // (Optional) trigger notification here
                return Ok(new { invitation.Id, invitation.ToEmail, invitation.CreatedAt });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("shared")]
        [Authorize]
        public IActionResult Shared()
        {
            var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? throw new UnauthorizedAccessException("User ID not found in token."));
            var shared = _passwordVaultService.ListSharedCredentials(userId, _context);
            return Ok(shared);
        }

        [HttpPost("share/accept")]
        [Authorize]
        public IActionResult AcceptShare(Guid invitationId)
        {
            var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? throw new UnauthorizedAccessException("User ID not found in token."));
            try
            {
                var credential = _passwordVaultService.AcceptShareInvitation(invitationId, userId, _context);
                return Ok(new { credential.Id, credential.Site, credential.Username, credential.Notes });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("share/reject")]
        [Authorize]
        public IActionResult RejectShare(Guid invitationId)
        {
            var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? throw new UnauthorizedAccessException("User ID not found in token."));
            try
            {
                _passwordVaultService.RejectShareInvitation(invitationId, userId, _context);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        #region Helper Methods
        private IActionResult? ValidateAndGetUser(out User? user)
        {
            user = null;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User not found." });

            user = _userService.GetById(Guid.Parse(userId));
            if (user == null)
                return NotFound(new { message = "User not found." });

            return null; // No error, user is valid
        }
        #endregion
    }
}