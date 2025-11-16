using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiUsuarios.Data;
using ApiUsuarios.Models;
using Microsoft.AspNetCore.Authorization;

namespace ApiUsuarios.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrador,Cajero,Inventario")]
    public class SucursalesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SucursalesController(AppDbContext context)
        {
            _context = context;
        }

        // 📋 GET: api/sucursales
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Sucursal>>> GetSucursales()
        {
            return await _context.Sucursales
                .Where(s => s.Activo)
                .OrderBy(s => s.Nombre)
                .ToListAsync();
        }

        // ➕ POST: api/sucursales
        [HttpPost]
        public async Task<ActionResult<Sucursal>> CrearSucursal(Sucursal sucursal)
        {
            sucursal.Activo = true;
            _context.Sucursales.Add(sucursal);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSucursales), new { id = sucursal.Id }, sucursal);
        }

        // ✏️ PUT: api/sucursales/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> EditarSucursal(int id, Sucursal sucursal)
        {
            if (id != sucursal.Id)
                return BadRequest();

            var dbSucursal = await _context.Sucursales.FindAsync(id);
            if (dbSucursal == null)
                return NotFound();

            dbSucursal.Nombre = sucursal.Nombre;
            dbSucursal.Direccion = sucursal.Direccion;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // 🚫 PATCH: api/sucursales/{id}/desactivar
        [HttpPatch("{id}/desactivar")]
        public async Task<IActionResult> DesactivarSucursal(int id)
        {
            var dbSucursal = await _context.Sucursales.FindAsync(id);
            if (dbSucursal == null)
                return NotFound();

            dbSucursal.Activo = false;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Sucursal desactivada" });
        }

        // ✅ PATCH: api/sucursales/{id}/reactivar
        [HttpPatch("{id}/reactivar")]
        public async Task<IActionResult> ReactivarSucursal(int id)
        {
            var dbSucursal = await _context.Sucursales.FindAsync(id);
            if (dbSucursal == null)
                return NotFound();

            dbSucursal.Activo = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Sucursal reactivada" });
        }
    }
}