using Domurion.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Domurion.Dtos;

namespace Domurion.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VaultController(IPasswordVaultService passwordVaultService) : ControllerBase
    {
        private readonly IPasswordVaultService _passwordVaultService = passwordVaultService;

        [HttpPost("add")]
        [Authorize]
        public IActionResult Add(Guid userId, string site, string username, string password, string? notes = null)
        {
            try
            {
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
                var credential = _passwordVaultService.AddCredential(userId, site, username, password, notes, ip);
                return Ok(new { credential.Id, credential.Site, credential.Username });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("list")]
        [Authorize]
        public IActionResult List(Guid userId)
        {
            var credentials = _passwordVaultService.GetCredentials(userId)
                .Select(c => new { c.Id, c.Site, c.Username });
            return Ok(credentials);
        }

        [HttpGet("retrieve")]
        [Authorize]
        public IActionResult Retrieve(Guid credentialId, Guid userId)
        {
            try
            {
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
                var password = _passwordVaultService.RetrievePassword(credentialId, userId, ip);
                return Ok(new { Password = password });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        [HttpPut("update")]
        [Authorize]
        public IActionResult Update(Guid credentialId, Guid userId, string? site, string? username, string? password, string? notes = null)
        {
            try
            {
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
                var credential = _passwordVaultService.UpdateCredential(credentialId, userId, site, username, password, notes, ip);
                return Ok(new { credential.Id, credential.Site, credential.Username, credential.Notes });
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
        public IActionResult Delete(Guid credentialId, Guid userId)
        {
            try
            {
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
                _passwordVaultService.DeleteCredential(credentialId, userId, ip);
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
        public IActionResult Export(Guid userId)
        {
            var credentials = _passwordVaultService.GetCredentials(userId)
                .Select(c => new
                {
                    c.Site,
                    c.Username,
                    Password = _passwordVaultService.RetrievePassword(c.Id, userId),
                });
            return Ok(credentials);
        }


        [HttpPost("import")]
        [Authorize]
        public IActionResult Import(Guid userId, [FromBody] List<ImportCredentialDto> credentials)
        {
            var imported = new List<object>();
            foreach (var cred in credentials)
            {
                try
                {
                    var c = _passwordVaultService.AddCredential(userId, cred.Site, cred.Username, cred.Password);
                    imported.Add(new { c.Id, c.Site, c.Username });
                }
                catch (Exception ex)
                {
                    imported.Add(new { cred.Site, cred.Username, error = ex.Message });
                }
            }
            return Ok(imported);
        }
    }
}