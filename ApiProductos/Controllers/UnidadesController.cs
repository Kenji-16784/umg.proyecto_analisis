using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using ApiProductos.Data;
using ApiProductos.Models;

namespace ApiProductos.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrador,Inventario")] // Solo roles con permiso de catálogo
    public class UnidadesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UnidadesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/unidades
        [HttpGet]
        [AllowAnonymous] // Todos pueden ver unidades para formularios
        public async Task<ActionResult<IEnumerable<Unidad>>> GetUnidades()
        {
            return await _context.Unidades.ToListAsync();
        }

        // GET: api/unidades/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Unidad>> GetUnidad(int id)
        {
            var unidad = await _context.Unidades.FindAsync(id);
            if (unidad == null)
                return NotFound();

            return unidad;
        }

        // POST: api/unidades
        [HttpPost]
        public async Task<ActionResult<Unidad>> CrearUnidad(Unidad unidad)
        {
            _context.Unidades.Add(unidad);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetUnidad), new { id = unidad.Id }, unidad);
        }

        // PUT: api/unidades/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> EditarUnidad(int id, Unidad unidad)
        {
            if (id != unidad.Id)
                return BadRequest();

            var existente = await _context.Unidades.FindAsync(id);
            if (existente == null)
                return NotFound();

            existente.Nombre = unidad.Nombre;
            existente.Abreviatura = unidad.Abreviatura;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/unidades/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarUnidad(int id)
        {
            var unidad = await _context.Unidades.FindAsync(id);
            if (unidad == null)
                return NotFound();

            // Evitar eliminar si tiene presentaciones relacionadas
            var tieneRelacion = await _context.Presentaciones.AnyAsync(p => p.UnidadId == id);
            if (tieneRelacion)
                return BadRequest("No se puede eliminar la unidad porque está asociada a una presentación.");

            _context.Unidades.Remove(unidad);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PATCH: api/unidades/{id}/desactivar
        [HttpPatch("{id}/desactivar")]
        public async Task<IActionResult> Desactivar(int id)
        {
            var unidad = await _context.Unidades.FindAsync(id);
            if (unidad == null)
                return NotFound();

            // ✅ Verificar si está en uso por productos activos
            bool enUso = await _context.Presentaciones
                .AnyAsync(p => p.UnidadId == id &&
                               _context.Productos.Any(prod => prod.Id == p.ProductoId && prod.Activo));

            if (enUso)
                return BadRequest("❌ No se puede desactivar la unidad: está asociada a productos activos.");

            unidad.Activo = false;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // PATCH: api/unidades/{id}/reactivar
        [HttpPatch("{id}/reactivar")]
        public async Task<IActionResult> Reactivar(int id)
        {
            var unidad = await _context.Unidades.FindAsync(id);
            if (unidad == null) return NotFound();

            unidad.Activo = true;
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
