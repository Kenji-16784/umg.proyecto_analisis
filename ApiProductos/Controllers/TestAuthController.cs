using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace ApiProductos.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestAuthController : ControllerBase
    {
        [HttpGet("claims")]
        [Authorize] // 🔐 Requiere token válido
        public IActionResult GetClaims()
        {
            var claims = User.Claims.Select(c => new
            {
                c.Type,
                c.Value
            });

            return Ok(new
            {
                Usuario = User.Identity?.Name ?? "Desconocido",
                Autenticado = User.Identity?.IsAuthenticated ?? false,
                Claims = claims
            });
        }
    }
}