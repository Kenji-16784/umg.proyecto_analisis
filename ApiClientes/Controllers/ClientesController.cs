using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiClientes.Data;
using ApiClientes.Models;
using Microsoft.AspNetCore.Authorization;

namespace ApiClientes.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // requiere token por defecto
    public class ClientesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ClientesController(AppDbContext context)
        {
            _context = context;
        }

        // =======================================
        // 🔍 Buscar cliente por NIT
        // =======================================
        [HttpGet("buscar/{nit}")]
        [AllowAnonymous] // ✅ Permite buscar desde POS sin token
        public async Task<ActionResult<Cliente>> BuscarClientePorNit(string nit)
        {
            if (string.IsNullOrWhiteSpace(nit))
                return BadRequest("❌ NIT inválido.");

            var cliente = await _context.Clientes
                .Include(c => c.ReglaPrecio) // 🟢 Incluye relación
                .FirstOrDefaultAsync(c => c.NIT != null && c.NIT.ToLower().Trim() == nit.ToLower().Trim());

            if (cliente == null)
                return NotFound("❌ Cliente no encontrado.");

            return Ok(cliente);
        }

        // =======================================
        // 📋 GET: api/clientes
        // =======================================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Cliente>>> GetClientes()
        {
            // 🟢 Incluye las reglas de precio también en el listado
            return await _context.Clientes
                .Include(c => c.ReglaPrecio)
                .ToListAsync();
        }

        // =======================================
        // 📄 GET: api/clientes/{id}
        // =======================================
        [HttpGet("{id}")]
        public async Task<ActionResult<Cliente>> GetCliente(int id)
        {
            var cliente = await _context.Clientes
                .Include(c => c.ReglaPrecio) // 🟢 Incluye la relación
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cliente == null)
                return NotFound("Cliente no encontrado.");

            return cliente;
        }

        // =======================================
        // ➕ POST: api/clientes
        // =======================================
        [HttpPost]
        [AllowAnonymous] // ✅ Permite crear cliente desde POS sin token
        public async Task<ActionResult<Cliente>> CrearCliente(Cliente cliente)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // ⚙️ Si no se envía ReglaPrecioId, asignar “Cliente Final” (ID 1)
            if (cliente.ReglaPrecioId <= 0)
                cliente.ReglaPrecioId = 1;

            // ⚙️ Validar NIT duplicado
            bool existeNit = await _context.Clientes.AnyAsync(c =>
                c.NIT != null && c.NIT.ToLower().Trim() == cliente.NIT.ToLower().Trim());
            if (existeNit)
                return Conflict($"Ya existe un cliente con el NIT {cliente.NIT}");

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();

            // 🟢 Devolver cliente con su regla de precio incluida
            var clienteCreado = await _context.Clientes
                .Include(c => c.ReglaPrecio)
                .FirstOrDefaultAsync(c => c.Id == cliente.Id);

            return CreatedAtAction(nameof(GetCliente), new { id = cliente.Id }, clienteCreado);
        }

        // =======================================
        // ✏️ PUT: api/clientes/{id}
        // =======================================
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador,Precios")]
        public async Task<IActionResult> EditarCliente(int id, Cliente cliente)
        {
            if (id != cliente.Id)
                return BadRequest("El ID no coincide.");

            var existente = await _context.Clientes.FindAsync(id);
            if (existente == null)
                return NotFound("Cliente no encontrado.");

            existente.Nombre = cliente.Nombre;
            existente.NIT = cliente.NIT;
            existente.Telefono = cliente.Telefono;
            existente.Direccion = cliente.Direccion;
            existente.Activo = cliente.Activo;

            // ✅ Solo Administrador puede cambiar regla de precio
            if (User.IsInRole("Administrador"))
                existente.ReglaPrecioId = cliente.ReglaPrecioId;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // =======================================
        // 🚫 PATCH: api/clientes/{id}/desactivar
        // =======================================
        [HttpPatch("{id}/desactivar")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DesactivarCliente(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null)
                return NotFound("Cliente no encontrado.");

            cliente.Activo = false;
            await _context.SaveChangesAsync();
            return Ok("Cliente desactivado correctamente.");
        }

        // =======================================
        // ✅ PATCH: api/clientes/{id}/reactivar
        // =======================================
        [HttpPatch("{id}/reactivar")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> ReactivarCliente(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null)
                return NotFound("Cliente no encontrado.");

            cliente.Activo = true;
            await _context.SaveChangesAsync();
            return Ok("Cliente reactivado correctamente.");
        }

        // =======================================
        // 🔄 PATCH: api/clientes/{id}/regla-precio
        // =======================================
        [HttpPatch("{id}/regla-precio")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> CambiarReglaPrecio(int id, [FromBody] int reglaPrecioId)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null)
                return NotFound("Cliente no encontrado.");

            cliente.ReglaPrecioId = reglaPrecioId;
            await _context.SaveChangesAsync();

            return Ok($"Regla de precio actualizada a ID {reglaPrecioId}.");
        }
    }
}