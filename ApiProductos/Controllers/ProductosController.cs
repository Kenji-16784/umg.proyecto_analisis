using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiProductos.Data;
using ApiProductos.Models;
using Microsoft.AspNetCore.Authorization;

namespace ApiProductos.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductosController(AppDbContext context)
        {
            _context = context;
        }

        // =======================================
        // 🔍 Buscar producto por código de barras
        // =======================================
        [HttpGet("buscar/{codigo}")]
        [Authorize(Roles = "Administrador,Cajero,Inventario")]
        public async Task<ActionResult<object>> BuscarPorCodigo(string codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo))
                return BadRequest("Código inválido.");

            var stock = await _context.StockProductos
                .Include(s => s.Producto)
                .FirstOrDefaultAsync(s =>
                    s.CodigoAlterno == codigo ||
                    s.ProductoId.ToString() == codigo);

            if (stock == null)
                return NotFound("Producto no encontrado.");

            await _context.Entry(stock.Producto)
                .Collection(p => p.Presentaciones)
                .Query()
                .Include(pr => pr.Unidad)
                .LoadAsync();

            var presentaciones = stock.Producto.Presentaciones?.Select(pr => new
            {
                pr.Id,
                pr.Tipo,
                pr.FactorConversion,
                Unidad = pr.Unidad != null ? pr.Unidad.Nombre : "Unidad"
            }).ToList();

            return Ok(new
            {
                stock.ProductoId,
                Nombre = stock.Producto?.Nombre,
                stock.CodigoAlterno,
                stock.Lote,
                stock.StockActual,
                stock.Costo,
                stock.PrecioVenta,
                stock.Producto?.Descripcion,
                Presentaciones = presentaciones
            });
        }

        // =======================================
        // 📋 GET: api/productos
        // =======================================
        [HttpGet]
        [Authorize(Roles = "Administrador,Cajero,Inventario")]
        public async Task<ActionResult<IEnumerable<Producto>>> GetProductos()
        {
            return await _context.Productos
                .Include(p => p.Presentaciones)
                .ThenInclude(pr => pr.Unidad)
                .ToListAsync();
        }

        // =======================================
        // 📄 GET: api/productos/{id}
        // =======================================
        [HttpGet("{id}")]
        [Authorize(Roles = "Administrador,Cajero,Inventario")]
        public async Task<ActionResult<Producto>> GetProducto(int id)
        {
            var producto = await _context.Productos
                .Include(p => p.Presentaciones)
                .ThenInclude(pr => pr.Unidad)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (producto == null)
                return NotFound();

            return producto;
        }

        // =======================================
        // ➕ POST: api/productos
        // =======================================
        [HttpPost]
        [Authorize(Roles = "Administrador,Inventario")]
        public async Task<ActionResult<Producto>> CrearProducto(Producto producto)
        {
            if (producto == null)
                return BadRequest("Datos de producto inválidos.");

            // ✅ Validar unidades antes de guardar
            foreach (var pre in producto.Presentaciones ?? new List<Presentacion>())
            {
                var unidad = await _context.Unidades.FindAsync(pre.UnidadId);
                if (unidad == null || !unidad.Activo)
                    return BadRequest($"La unidad ID {pre.UnidadId} no existe o está inactiva.");
            }

            // ✅ Limpiar IDs de presentaciones (evita error IDENTITY_INSERT)
            if (producto.Presentaciones != null)
            {
                foreach (var pre in producto.Presentaciones)
                {
                    pre.Id = 0; // ⚡ Fuerza autoincremento
                    pre.ProductoId = 0; // ⚡ Limpia el vínculo hasta que se guarde
                }
            }

            // 🔹 Guardar el producto principal
            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();

            // 🔹 Guardar las presentaciones ligadas
            if (producto.Presentaciones != null)
            {
                foreach (var pre in producto.Presentaciones)
                {
                    pre.ProductoId = producto.Id; // asigna el producto recién creado
                    pre.Id = 0; // asegura autoincremento
                    _context.Presentaciones.Add(pre);
                }
                await _context.SaveChangesAsync();
            }

            // 🔹 Recargar el producto con presentaciones para retornar al frontend
            await _context.Entry(producto)
                .Collection(p => p.Presentaciones)
                .Query()
                .Include(pr => pr.Unidad)
                .LoadAsync();

            return CreatedAtAction(nameof(GetProducto), new { id = producto.Id }, producto);
        }

        // =======================================
        // ✏️ PUT: api/productos/{id}
        // =======================================
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador,Inventario")]
        public async Task<IActionResult> EditarProducto(int id, Producto producto)
        {
            if (id != producto.Id)
                return BadRequest("El ID no coincide.");

            var existente = await _context.Productos
                .Include(p => p.Presentaciones)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (existente == null)
                return NotFound("Producto no encontrado.");

            foreach (var pre in producto.Presentaciones ?? new List<Presentacion>())
            {
                var unidad = await _context.Unidades.FindAsync(pre.UnidadId);
                if (unidad == null || !unidad.Activo)
                    return BadRequest($"La unidad con ID {pre.UnidadId} no existe o está inactiva.");
            }

            existente.Nombre = producto.Nombre;
            existente.Descripcion = producto.Descripcion;
            existente.Activo = producto.Activo;

            // 🔄 Reemplazar presentaciones (limpiando IDs)
            _context.Presentaciones.RemoveRange(existente.Presentaciones);
            await _context.SaveChangesAsync();

            if (producto.Presentaciones != null)
            {
                foreach (var pre in producto.Presentaciones)
                {
                    pre.Id = 0; // ⚡ Se asegura el autoincremento
                    pre.ProductoId = existente.Id;
                    _context.Presentaciones.Add(pre);
                }
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // =======================================
        // 🚫 PATCH: api/productos/{id}/desactivar
        // =======================================
        [HttpPatch("{id}/desactivar")]
        [Authorize(Roles = "Administrador,Inventario")]
        public async Task<IActionResult> DesactivarProducto(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
                return NotFound();

            producto.Activo = false;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // =======================================
        // ✅ PATCH: api/productos/{id}/reactivar
        // =======================================
        [HttpPatch("{id}/reactivar")]
        [Authorize(Roles = "Administrador,Inventario")]
        public async Task<IActionResult> ReactivarProducto(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
                return NotFound();

            producto.Activo = true;
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}