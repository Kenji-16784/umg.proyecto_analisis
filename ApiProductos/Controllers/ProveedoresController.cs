using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using ApiProductos.Data;
using ApiProductos.Models;

namespace ApiProductos.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrador,Inventario")]
    public class ProveedoresController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProveedoresController(AppDbContext context)
        {
            _context = context;
        }

        // === GET: api/Proveedores ===
        [HttpGet]
        [Authorize(Roles = "Administrador,Inventario,Cajero")]
        public async Task<ActionResult<IEnumerable<Proveedor>>> GetProveedores()
        {
            return await _context.Proveedores
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }

        // === GET: api/Proveedores/{id} ===
        [HttpGet("{id}")]
        [Authorize(Roles = "Administrador,Inventario,Cajero")]
        public async Task<ActionResult<Proveedor>> GetProveedor(int id)
        {
            var proveedor = await _context.Proveedores.FindAsync(id);
            if (proveedor == null)
                return NotFound("Proveedor no encontrado.");

            return proveedor;
        }

        // === POST: api/Proveedores ===
        [HttpPost]
        [Authorize(Roles = "Administrador,Inventario")]
        public async Task<ActionResult<Proveedor>> CrearProveedor(Proveedor proveedor)
        {
            if (string.IsNullOrWhiteSpace(proveedor.Nombre) || string.IsNullOrWhiteSpace(proveedor.Nit))
                return BadRequest("Debe proporcionar nombre y NIT del proveedor.");

            // 🔎 Evitar duplicados por NIT
            var existente = await _context.Proveedores
                .FirstOrDefaultAsync(p => p.Nit == proveedor.Nit);

            if (existente != null)
                return Conflict($"Ya existe un proveedor con el NIT {proveedor.Nit}.");

            proveedor.Activo = true;
            _context.Proveedores.Add(proveedor);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProveedor), new { id = proveedor.Id }, proveedor);
        }

        // === PUT: api/Proveedores/{id} ===
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador,Inventario")]
        public async Task<IActionResult> EditarProveedor(int id, Proveedor proveedor)
        {
            if (id != proveedor.Id)
                return BadRequest("El ID de la URL no coincide con el del modelo.");

            var existente = await _context.Proveedores.FindAsync(id);
            if (existente == null)
                return NotFound("Proveedor no encontrado.");

            existente.Nombre = proveedor.Nombre;
            existente.Nit = proveedor.Nit;
            existente.Telefono = proveedor.Telefono;
            existente.Correo = proveedor.Correo;
            existente.Direccion = proveedor.Direccion;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // === PATCH: api/Proveedores/{id}/desactivar ===
        [HttpPatch("{id}/desactivar")]
        [Authorize(Roles = "Administrador,Inventario")]
        public async Task<IActionResult> DesactivarProveedor(int id)
        {
            var proveedor = await _context.Proveedores.FindAsync(id);
            if (proveedor == null)
                return NotFound("Proveedor no encontrado.");

            proveedor.Activo = false;
            await _context.SaveChangesAsync();

            return Ok("Proveedor desactivado correctamente.");
        }

        // === PATCH: api/Proveedores/{id}/reactivar ===
        [HttpPatch("{id}/reactivar")]
        [Authorize(Roles = "Administrador,Inventario")]
        public async Task<IActionResult> ReactivarProveedor(int id)
        {
            var proveedor = await _context.Proveedores.FindAsync(id);
            if (proveedor == null)
                return NotFound("Proveedor no encontrado.");

            proveedor.Activo = true;
            await _context.SaveChangesAsync();

            return Ok("Proveedor reactivado correctamente.");
        }
    }
}