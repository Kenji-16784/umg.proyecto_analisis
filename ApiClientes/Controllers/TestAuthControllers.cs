using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace ApiClientes.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestAuthController : ControllerBase
    {
        [HttpGet("claims")]
        [Authorize]
        public IActionResult GetClaims()
        {
            var user = HttpContext.User;

            if (user?.Identity?.IsAuthenticated != true)
                return Unauthorized(new { message = "Token no válido o no autenticado" });

            var claims = user.Claims.Select(c => new { type = c.Type, value = c.Value });

            return Ok(new
            {
                usuario = user.Identity?.Name,
                autenticado = user.Identity?.IsAuthenticated,
                claims
            });
        }
    }
}