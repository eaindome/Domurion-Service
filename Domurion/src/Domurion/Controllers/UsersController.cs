using Microsoft.AspNetCore.Mvc;
using Domurion.Services;
using Domurion.Dtos;
using Microsoft.AspNetCore.RateLimiting;

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
                return Ok(new { user.Id, user.Username });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}