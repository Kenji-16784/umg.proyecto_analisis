using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ApiUsuarios.Data;
using ApiUsuarios.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiUsuarios.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrador")]
    [Produces("application/json")]
    public class UsuariosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsuariosController(AppDbContext context)
        {
            _context = context;
        }

        // 📋 GET: api/usuarios
        [HttpGet]
        public async Task<IActionResult> GetUsuarios()
        {
            try
            {
                var usuarios = await _context.Usuarios
                    .Include(u => u.Rol)
                    .Include(u => u.Sucursal)
                    .Select(u => new
                    {
                        u.Id,
                        u.Nombre,
                        u.Email,
                        Rol = u.Rol != null ? u.Rol.Nombre : "Sin Rol",
                        Sucursal = u.Sucursal != null ? u.Sucursal.Nombre : "Sin Sucursal",
                        u.Activo,
                        u.RolId,
                        u.SucursalId
                    })
                    .ToListAsync();

                return Ok(usuarios);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // ➕ POST: api/usuarios
        [HttpPost]
        public async Task<ActionResult<Usuario>> CrearUsuario(Usuario usuario)
        {
            var rol = await _context.Roles.FindAsync(usuario.RolId);
            var sucursal = await _context.Sucursales.FindAsync(usuario.SucursalId);

            if (rol == null || !rol.Activo)
                return BadRequest("Rol inválido o inactivo.");

            if (sucursal == null || !sucursal.Activo)
                return BadRequest("Sucursal inválida o inactiva.");

            usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(usuario.PasswordHash);
            usuario.Activo = true;

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUsuarios), new { id = usuario.Id }, usuario);
        }

        // ✏️ PUT: api/usuarios/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> EditarUsuario(int id, Usuario usuario)
        {
            if (id != usuario.Id)
                return BadRequest();

            var dbUser = await _context.Usuarios.FindAsync(id);
            if (dbUser == null)
                return NotFound();

            dbUser.Nombre = usuario.Nombre;
            dbUser.Email = usuario.Email;
            dbUser.RolId = usuario.RolId;
            dbUser.SucursalId = usuario.SucursalId;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // 🚫 PATCH: api/usuarios/{id}/desactivar
        [HttpPatch("{id}/desactivar")]
        public async Task<IActionResult> DesactivarUsuario(int id)
        {
            var dbUser = await _context.Usuarios.FindAsync(id);
            if (dbUser == null)
                return NotFound();

            dbUser.Activo = false;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Usuario desactivado" });
        }

        // ✅ PATCH: api/usuarios/{id}/reactivar
        [HttpPatch("{id}/reactivar")]
        public async Task<IActionResult> ReactivarUsuario(int id)
        {
            var dbUser = await _context.Usuarios.FindAsync(id);
            if (dbUser == null)
                return NotFound();

            dbUser.Activo = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Usuario reactivado" });
        }
    }
}