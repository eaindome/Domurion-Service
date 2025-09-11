using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Domurion.Services;
using Domurion.Models;
using System.Security.Claims;

namespace Domurion.Controllers
{
    [ApiController]
    [Route("api/user/preferences")]
    [Authorize]
    public class PreferencesController(PreferencesService preferencesService) : ControllerBase
    {
        private readonly PreferencesService _preferencesService = preferencesService;

        [HttpGet]
        public ActionResult<UserPreferences> Get()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var prefs = _preferencesService.GetPreferences(Guid.Parse(userId));
            return Ok(prefs);
        }

        [HttpPut]
        public ActionResult<UserPreferences> Update([FromBody] UserPreferences updated)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var prefs = _preferencesService.UpdatePreferences(Guid.Parse(userId), updated);
            return Ok(prefs);
        }

        [HttpGet("generate-password")]
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
