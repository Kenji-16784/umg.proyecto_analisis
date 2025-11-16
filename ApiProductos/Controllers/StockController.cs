using ApiProductos.Data;
using ApiProductos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace ApiProductos.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StockController : ControllerBase
    {
        private readonly AppDbContext _context;

        public StockController(AppDbContext context)
        {
            _context = context;
        }

        // ====================================================
        // 🔍 BUSCAR POR CÓDIGO ALTERNO
        // ====================================================
        [HttpGet("buscar/{codigo}")]
        public async Task<IActionResult> BuscarPorCodigo(string codigo)
        {
            try
            {
                var stock = await _context.StockProductos
                    .Include(s => s.Producto)
                        .ThenInclude(p => p.Presentaciones)
                            .ThenInclude(pre => pre.Unidad)
                    .Include(s => s.Sucursal)
                    .Include(s => s.Proveedor)
                    .FirstOrDefaultAsync(s => s.CodigoAlterno == codigo);

                if (stock == null)
                    return NotFound(new { mensaje = "No se encontró el producto con ese código." });

                return Ok(stock);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno en el servidor.", detalle = ex.Message });
            }
        }

        // ====================================================
        // 📦 GET: api/stock
        // ====================================================
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<object>>> GetStock(
            [FromQuery] int? sucursalId,
            [FromQuery] int? productoId)
        {
            var query = _context.StockProductos
                .Include(s => s.Producto)
                    .ThenInclude(p => p.Presentaciones)
                        .ThenInclude(pr => pr.Unidad)
                .Include(s => s.Proveedor)
                .Include(s => s.Sucursal)
                .AsQueryable();

            if (sucursalId.HasValue && sucursalId > 0)
                query = query.Where(s => s.SucursalId == sucursalId);

            if (productoId.HasValue && productoId > 0)
                query = query.Where(s => s.ProductoId == productoId);

var stock = await query
    .OrderBy(s => s.Sucursal!.Nombre)
    .ThenBy(s => s.Producto!.Nombre)
    .Select(s => new
    {
        s.Id,
        Producto = s.Producto != null ? s.Producto.Nombre : "—",
        Proveedor = s.Proveedor != null ? s.Proveedor.Nombre : "—",
        Sucursal = s.Sucursal != null ? s.Sucursal.Nombre : "—",
        s.CodigoAlterno,
        s.Lote,
        StockActual = s.StockActual,
        UnidadBase = s.Producto.Presentaciones.FirstOrDefault() != null
            ? (s.Producto.Presentaciones.FirstOrDefault().Unidad != null
                ? s.Producto.Presentaciones.FirstOrDefault().Unidad.Nombre
                : "—")
            : "—",
        s.Costo,
        s.PrecioVenta,

        // 🟢 Conversión segura de fecha
        FechaUltimaCompra = EF.Functions.IsDate(s.FechaUltimaCompra.ToString()) 
        ? s.FechaUltimaCompra 
        : DateTime.Now,

        s.Activo
    })
    .ToListAsync();

            return Ok(stock);
        }

        // ====================================================
        // 🔍 GET: api/stock/buscar
        // ====================================================
        [HttpGet("buscar")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<object>>> BuscarStock(
            [FromQuery] string? nombre = null,
            [FromQuery] string? proveedor = null,
            [FromQuery] string? sucursal = null)
        {
            var query = _context.StockProductos
                .Include(s => s.Producto)
                    .ThenInclude(p => p.Presentaciones)
                        .ThenInclude(pr => pr.Unidad)
                .Include(s => s.Proveedor)
                .Include(s => s.Sucursal)
                .AsQueryable();

            if (!string.IsNullOrEmpty(nombre))
                query = query.Where(s => s.Producto!.Nombre.Contains(nombre));

            if (!string.IsNullOrEmpty(proveedor))
                query = query.Where(s => s.Proveedor!.Nombre.Contains(proveedor));

            if (!string.IsNullOrEmpty(sucursal))
                query = query.Where(s => s.Sucursal!.Nombre.Contains(sucursal));

            var resultado = await query
                .Select(s => new
                {
                    s.Id,
                    Producto = s.Producto!.Nombre,
                    Proveedor = s.Proveedor!.Nombre,
                    Sucursal = s.Sucursal!.Nombre,
                    s.CodigoAlterno,
                    s.Lote,
                    StockActual = s.StockActual,
                    UnidadBase = s.Producto.Presentaciones.FirstOrDefault().Unidad.Nombre,
                    s.Costo,
                    s.PrecioVenta,
                    s.FechaUltimaCompra,
                    s.Activo
                })
                .ToListAsync();

            return Ok(resultado);
        }

        // ====================================================
        // 📜 GET: api/stock/movimientos/{productoId}
        // ====================================================
        [HttpGet("movimientos/{stockId}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<object>>> GetMovimientos(int stockId)
        {
            try
            {
                var stock = await _context.StockProductos
                    .Include(s => s.Producto)
                    .Include(s => s.Sucursal)
                    .FirstOrDefaultAsync(s => s.Id == stockId);

                if (stock == null)
                    return BadRequest("Stock no encontrado.");

                int productoId = stock.ProductoId;

                // 🟢 COMPRAS
                var compras = await _context.Compras
                    .Include(c => c.Detalles)
                    .Include(c => c.Sucursal)
                    .Where(c => c.Detalles.Any(d => d.ProductoId == productoId))
                    .SelectMany(c => c.Detalles
                        .Where(d => d.ProductoId == productoId)
                        .Select(d => new
                        {
                            Tipo = "Compra",
                            Fecha = c.FechaCompra,
                            Cantidad = d.Cantidad,
                            PrecioUnitario = d.PrecioCompra,
                            Sucursal = c.Sucursal != null ? c.Sucursal.Nombre : "—",
                            Observaciones = c.Observaciones ?? ""
                        }))
                    .ToListAsync();

                // 🟣 VENTAS
                var ventas = await _context.Ventas
                    .Include(v => v.Detalles)
                    .Include(v => v.Sucursal)
                    .Where(v => v.Detalles.Any(d => d.ProductoId == productoId))
                    .SelectMany(v => v.Detalles
                        .Where(d => d.ProductoId == productoId)
                        .Select(d => new
                        {
                            Tipo = "Venta",
                            Fecha = v.FechaVenta,
                            Cantidad = d.Cantidad,
                            PrecioUnitario = d.PrecioUnitario,
                            Sucursal = v.Sucursal != null ? v.Sucursal.Nombre : "—",
                            Observaciones = v.Observaciones ?? ""
                        }))
                    .ToListAsync();

                var movimientos = compras.Concat(ventas).OrderByDescending(m => m.Fecha).ToList();
                return Ok(movimientos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener movimientos.", detalle = ex.Message });
            }
        }

        // ====================================================
        // 🚫 PATCH: api/stock/{id}/toggle
        // ====================================================
        [HttpPatch("{id}/toggle")]
        [Authorize(Roles = "Administrador,EncargadoUsuarios")]
        public async Task<IActionResult> ToggleEstado(int id)
        {
            var stock = await _context.StockProductos.FindAsync(id);
            if (stock == null) return NotFound("❌ Stock no encontrado.");

            stock.Activo = !stock.Activo;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = stock.Activo
                    ? "✅ Stock activado nuevamente."
                    : "🚫 Stock desactivado correctamente."
            });
        }

        // ====================================================
        // ✅ NUEVO: VERIFICAR STOCK DISPONIBLE PARA VENTA
        // ====================================================
        [HttpGet("verificar/{productoId:int}/{sucursalId:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> VerificarStock(int productoId, int sucursalId)
        {
            var stock = await _context.StockProductos
                .Include(s => s.Producto)
                .FirstOrDefaultAsync(s => s.ProductoId == productoId && s.SucursalId == sucursalId);

            if (stock == null)
                return NotFound(new { message = "No existe registro de stock para este producto." });

            return Ok(new
            {
                stock.ProductoId,
                Producto = stock.Producto?.Nombre ?? "—",
                stock.SucursalId,
                stock.StockActual
            });
        }
    }
}