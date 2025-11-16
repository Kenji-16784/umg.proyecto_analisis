using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using ApiProductos.Data;
using ApiProductos.Models;
using System.Text.Json;

namespace ApiProductos.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReglasPrecioController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReglasPrecioController(AppDbContext context)
        {
            _context = context;
        }

        // ========================================
        // 📋 GET: api/ReglasPrecio
        // ========================================
        [HttpGet]
        [Authorize(Roles = "Administrador,Inventario,Cajero")]
        public async Task<ActionResult<IEnumerable<ReglaPrecio>>> GetReglas()
        {
            return await _context.ReglasPrecio
                .OrderBy(r => r.TipoCliente)
                .ToListAsync();
        }

        // ========================================
        // 🔍 GET: api/ReglasPrecio/{id}
        // ========================================
        [HttpGet("{id}")]
        [Authorize(Roles = "Administrador,Inventario,Cajero")]
        public async Task<ActionResult<ReglaPrecio>> GetRegla(int id)
        {
            var regla = await _context.ReglasPrecio.FindAsync(id);
            if (regla == null)
                return NotFound(new { message = "❌ Regla no encontrada." });
            return regla;
        }

        // ========================================
        // ➕ POST: api/ReglasPrecio
        // ========================================
        [HttpPost]
        [Authorize(Roles = "Administrador,Inventario")]
        public async Task<ActionResult<ReglaPrecio>> PostRegla(ReglaPrecio regla)
        {
            if (string.IsNullOrWhiteSpace(regla.TipoCliente))
                return BadRequest("❌ El tipo de cliente es obligatorio.");

            _context.ReglasPrecio.Add(regla);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetRegla), new { id = regla.Id }, regla);
        }

        // ========================================
        // ✏️ PUT: api/ReglasPrecio/{id}
        // ========================================
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador,Inventario")]
        public async Task<IActionResult> PutRegla(int id, [FromBody] JsonElement body)
        {
            try
            {
                var regla = await _context.ReglasPrecio.FindAsync(id);
                if (regla == null)
                    return NotFound(new { message = "❌ Regla no encontrada." });

                // ✅ Actualiza solo los campos enviados
                if (body.TryGetProperty("tipoCliente", out var tipoClienteProp))
                    regla.TipoCliente = tipoClienteProp.GetString() ?? regla.TipoCliente;

                if (body.TryGetProperty("porcentaje", out var porcentajeProp))
                    regla.Porcentaje = porcentajeProp.GetDecimal();

                if (body.TryGetProperty("activo", out var activoProp))
                    regla.Activo = activoProp.GetBoolean();

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "✅ Regla actualizada correctamente.",
                    id = regla.Id,
                    tipoCliente = regla.TipoCliente,
                    porcentaje = regla.Porcentaje,
                    activo = regla.Activo
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "❌ Error al actualizar la regla.", detalle = ex.Message });
            }
        }

        // ========================================
        // 🔄 PATCH: api/ReglasPrecio/{id}/estado
        // ========================================
        [HttpPatch("{id}/estado")]
        [Authorize(Roles = "Administrador,Inventario")]
        public async Task<IActionResult> CambiarEstado(int id, [FromBody] JsonElement body)
        {
            try
            {
                if (!body.TryGetProperty("activo", out var activoProp))
                    return BadRequest(new { message = "❌ Debe incluir el campo 'activo'." });

                bool nuevoEstado = activoProp.GetBoolean();

                var regla = await _context.ReglasPrecio.FindAsync(id);
                if (regla == null)
                    return NotFound(new { message = "❌ Regla no encontrada." });

                regla.Activo = nuevoEstado;
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = nuevoEstado
                        ? "✅ Regla activada correctamente."
                        : "🚫 Regla desactivada correctamente.",
                    id = regla.Id,
                    activo = regla.Activo
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "❌ Error al cambiar estado.", detalle = ex.Message });
            }
        }

        // ========================================
        // 🗑️ DELETE: api/ReglasPrecio/{id}
        // ========================================
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteRegla(int id)
        {
            var regla = await _context.ReglasPrecio.FindAsync(id);
            if (regla == null)
                return NotFound(new { message = "❌ Regla no encontrada." });

            _context.ReglasPrecio.Remove(regla);
            await _context.SaveChangesAsync();

            return Ok(new { message = "🗑️ Regla eliminada correctamente." });
        }
    }
}