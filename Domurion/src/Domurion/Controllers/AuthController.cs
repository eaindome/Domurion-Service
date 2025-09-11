using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Domurion.Helpers;
using Domurion.Services.Interfaces;
using Domurion.Models;
using System.Security.Claims;

namespace Domurion.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;

        public AuthController(IUserService userService, IConfiguration configuration)
        {
            _userService = userService;
            _configuration = configuration;
        }

        [HttpGet("google-login")]
        public IActionResult GoogleLogin(string? returnUrl = null)
        {
            var redirectUrl = Url.Action("GoogleResponse", "Auth", new { returnUrl });
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("/signin-google")]
        [AllowAnonymous]
        public async Task<IActionResult> GoogleResponse(string? returnUrl = null)
        {
            var authenticateResult = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
            if (!authenticateResult.Succeeded || authenticateResult.Principal == null)
                return Unauthorized();

            var email = authenticateResult.Principal.FindFirst(ClaimTypes.Email)?.Value;
            var name = authenticateResult.Principal.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(email))
                return BadRequest("Google account email not found.");

            // Find or create user in your DB
            var user = _userService.GetByUsername(email);
            user ??= _userService.CreateExternalUser(email, name, "Google");

            // Generate JWT for your app
            var token = JwtHelper.GenerateJwtToken(user, _configuration);

            // Optionally, redirect to frontend with token as query param
            if (!string.IsNullOrEmpty(returnUrl))
                return Redirect(returnUrl + "?token=" + token);

            return Ok(new { token });
        }
    }
}
