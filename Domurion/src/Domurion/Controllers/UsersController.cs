using Domurion.Dtos;
using Domurion.Helpers;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Domurion.Services.Interfaces;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Authorization;

namespace Domurion.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableRateLimiting("fixed")]
    public class UsersController(IUserService userService) : ControllerBase
    {
        private readonly IUserService _userService = userService;

        [HttpPost("register")]
        public IActionResult Register([FromBody] UserDto userDto)
        {
            try
            {
                var user = _userService.Register(userDto.Username, userDto.Password);
                return Ok(new { user.Id, user.Username });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { error = ex.Message });
            }
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] UserDto userDto)
        {
            try
            {
                var user = _userService.Login(userDto.Username, userDto.Password);
                if (user == null)
                    return Unauthorized(new { error = "Invalid username or password." });

                // Generate JWT token
                var token = Helpers.JwtHelper.GenerateJwtToken(user, HttpContext.RequestServices.GetService<IConfiguration>()!);
                return Ok(new { user.Id, user.Username, token });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("update")]
        public IActionResult Update(Guid userId, string? newUsername, string? newPassword)
        {
            try
            {
                var user = _userService.UpdateUser(userId, newUsername, newPassword);
                return Ok(new { user.Id, user.Username });
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
                return Conflict(new { error = ex.Message });
            }
        }

        [HttpGet("generate-password")]
        public IActionResult GeneratePassword([FromQuery] int length = 16)
        {
            var password = Helper.GeneratePassword(length);
            return Ok(new { password });
        }

        [HttpDelete("delete")]
        public IActionResult Delete(Guid userId)
        {
            try
            {
                _userService.DeleteUser(userId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        [HttpPost("link-google")]
        [Authorize]
        public IActionResult LinkGoogle([FromBody] string googleId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            var result = _userService.LinkGoogleAccount(Guid.Parse(userId), googleId);
            if (!result)
                return BadRequest("Google account already linked to another user.");
            return Ok("Google account linked successfully.");
        }

        [HttpPost("unlink-google")]
        [Authorize]
        public IActionResult UnlinkGoogle()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            var result = _userService.UnlinkGoogleAccount(Guid.Parse(userId));
            if (!result)
                return BadRequest("No Google account to unlink.");
            return Ok("Google account unlinked successfully.");
        }
    }
}