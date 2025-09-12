using Microsoft.AspNetCore.Mvc;

namespace Domurion.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController(Data.DataContext context) : ControllerBase
    {
        private readonly Data.DataContext _context = context;

        // Backend health only
        [HttpGet]
        [Route("")]
        public IActionResult Get()
        {
            return Ok(new { status = "ok", timestamp = DateTime.UtcNow });
        }

        // Database connectivity health
        [HttpGet("db")]
        public IActionResult DbHealth()
        {
            try
            {
                var canConnect = _context.Database.CanConnect();
                if (!canConnect)
                {
                    return StatusCode(503, new { status = "unhealthy", db = "unreachable", timestamp = DateTime.UtcNow });
                }
                _ = _context.Users.Take(1).ToList();
            }
            catch (Exception ex)
            {
                return StatusCode(503, new { status = "unhealthy", db = ex.Message, timestamp = DateTime.UtcNow });
            }
            return Ok(new { status = "ok", db = "ok", timestamp = DateTime.UtcNow });
        }
    }
}
