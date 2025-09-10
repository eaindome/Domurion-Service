using Domurion.Services.Interfaces;

namespace Domurion.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VaultController : ControllerBase
    {
        private readonly IPasswordVaultService _passwordVaultService;

        public VaultController(IPasswordVaultService passwordVaultService)
        {
            _passwordVaultService = passwordVaultService;
        }

        [HttpPost("add")]
        public IActionResult Add(Guid userId, string site, string username, string password)
        {
            try
            {
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
                var credential = _passwordVaultService.AddCredential(userId, site, username, password, ip);
                return Ok(new { credential.Id, credential.Site, credential.Username });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("list")]
        public IActionResult List(Guid userId)
        {
            var credentials = _passwordVaultService.GetCredentials(userId)
                .Select(c => new { c.Id, c.Site, c.Username });
            return Ok(credentials);
        }

        [HttpGet("retrieve")]
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
        public IActionResult Update(Guid credentialId, Guid userId, string? site, string? username, string? password)
        {
            try
            {
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
                var credential = _passwordVaultService.UpdateCredential(credentialId, userId, site, username, password, ip);
                return Ok(new { credential.Id, credential.Site, credential.Username });
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
    }
}