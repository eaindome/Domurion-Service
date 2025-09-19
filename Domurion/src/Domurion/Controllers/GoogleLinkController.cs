using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Domurion.Services.Interfaces;
using System.Security.Claims;

namespace Domurion.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class GoogleLinkController : ControllerBase
    {
        private readonly IUserService _userService;
        public GoogleLinkController(IUserService userService)
        {
            _userService = userService;
        }

        // Initiate Google OAuth for linking
        [HttpGet("link-google-oauth")]
        [Authorize]
        public IActionResult LinkGoogleOAuth(string? returnUrl = null)
        {
            var redirectUrl = Url.Action("GoogleLinkCallback", "GoogleLink", new { returnUrl });
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        // Callback for Google OAuth linking
        [HttpGet("/link-google-callback")]
        [AllowAnonymous]
        public async Task<IActionResult> GoogleLinkCallback(string? returnUrl = null)
        {
            var authenticateResult = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
            if (!authenticateResult.Succeeded || authenticateResult.Principal == null)
                return Unauthorized(new { message = "Google authentication failed." });

            var googleId = authenticateResult.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(googleId))
                return BadRequest(new { message = "Google ID not found." });

            // Get the currently logged-in user from the JWT (from the original session)
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User not found." });

            var result = _userService.LinkGoogleAccount(Guid.Parse(userId), googleId);
            if (!result)
                return BadRequest(new { message = "Google account already linked to another user." });

            if (!string.IsNullOrEmpty(returnUrl))
                return Redirect(returnUrl + "?linked=true");

            return Ok(new { message = "Google account linked successfully." });
        }

        // Unlink Google account
        [HttpPost("unlink-google")]
        [Authorize]
        public IActionResult UnlinkGoogle()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User not found." });
            var result = _userService.UnlinkGoogleAccount(Guid.Parse(userId));
            if (!result)
                return BadRequest(new { message = "No Google account to unlink." });
            return Ok(new { message = "Google account unlinked successfully." });
        }
    }
}
