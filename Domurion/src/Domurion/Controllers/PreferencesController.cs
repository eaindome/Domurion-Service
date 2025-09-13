using Domurion.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Domurion.Models;
using System.Security.Claims;

namespace Domurion.Controllers
{
    [ApiController]
    [Route("api/user/preferences")]
    [Authorize]
    public class PreferencesController(IPreferencesService preferencesService) : ControllerBase
    {
        private readonly IPreferencesService _preferencesService = preferencesService;

        [HttpGet]
        [Authorize]
        public ActionResult<UserPreferences> Get()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var prefs = _preferencesService.GetPreferences(Guid.Parse(userId));
            return Ok(prefs);
        }

        [HttpPut]
        [Authorize]
        public ActionResult<UserPreferences> Update([FromBody] UserPreferences updated)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var prefs = _preferencesService.UpdatePreferences(Guid.Parse(userId), updated);
            return Ok(prefs);
        }

        [HttpGet("generate-password")]
        [Authorize]
        public ActionResult<string> GeneratePassword(
            [FromQuery] int? length,
            [FromQuery] bool? useUppercase,
            [FromQuery] bool? useLowercase,
            [FromQuery] bool? useNumbers,
            [FromQuery] bool? useSymbols)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var prefs = _preferencesService.GetPreferences(Guid.Parse(userId));
            var password = Domurion.Helpers.Helper.GeneratePassword(
                length ?? prefs.PasswordLength,
                useUppercase ?? prefs.UseUppercase,
                useLowercase ?? prefs.UseLowercase,
                useNumbers ?? prefs.UseNumbers,
                useSymbols ?? prefs.UseSymbols
            );
            return Ok(new { password });
        }
    }
}
