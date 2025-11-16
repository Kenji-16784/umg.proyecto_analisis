using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiUsuarios.Data;
using ApiUsuarios.Models;
using Microsoft.AspNetCore.Authorization;

namespace ApiUsuarios.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrador")]
    public class RolesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RolesController(AppDbContext context)
        {
            _context = context;
        }

        // 📋 GET: api/roles
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Rol>>> GetRoles()
        {
            return await _context.Roles
                .OrderBy(r => r.Nombre)
                .ToListAsync();
        }

        // ➕ POST: api/roles
        [HttpPost]
        public async Task<IActionResult> CrearRol([FromBody] Rol rol)
        {
            if (string.IsNullOrWhiteSpace(rol.Nombre))
                return BadRequest("El nombre del rol es obligatorio.");

            rol.Activo = true;
            _context.Roles.Add(rol);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Rol creado correctamente" });
        }

        // ✏️ PUT: api/roles/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> EditarRol(int id, [FromBody] Rol rol)
        {
            var dbRol = await _context.Roles.FindAsync(id);
            if (dbRol == null)
                return NotFound("Rol no encontrado");

            dbRol.Nombre = rol.Nombre;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Rol actualizado correctamente" });
        }

        // 🚫 PATCH: api/roles/{id}/desactivar
        [HttpPatch("{id}/desactivar")]
        public async Task<IActionResult> DesactivarRol(int id)
        {
            var rol = await _context.Roles.FindAsync(id);
            if (rol == null)
                return NotFound("Rol no encontrado");

            // 🧩 Verificar si algún usuario usa este rol
            bool rolEnUso = await _context.Usuarios.AnyAsync(u => u.RolId == id && u.Activo);
            if (rolEnUso)
                return BadRequest("❌ No se puede desactivar este rol porque está asignado a uno o más usuarios activos.");

            rol.Activo = false;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Rol desactivado correctamente" });
        }

        // ✅ PATCH: api/roles/{id}/reactivar
        [HttpPatch("{id}/reactivar")]
        public async Task<IActionResult> ReactivarRol(int id)
        {
            var rol = await _context.Roles.FindAsync(id);
            if (rol == null)
                return NotFound("Rol no encontrado");

            rol.Activo = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Rol reactivado correctamente" });
        }
    }
}