using Domurion.Models;
using Microsoft.AspNetCore.Mvc;
using Domurion.Services.Interfaces;

namespace Domurion.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VaultController(IPasswordVaultService passwordVaultService) : ControllerBase
    {
        private readonly IPasswordVaultService _passwordVaultService = passwordVaultService;

        [HttpPost("add")]
        public IActionResult Add(Guid userId, string site, string username, string password)
        {
            try
            {
                var credential = _passwordVaultService.AddCredential(userId, site, username, password);
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
                var password = _passwordVaultService.RetrievePassword(credentialId, userId);
                return Ok(new { Password = password });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }
    }
}