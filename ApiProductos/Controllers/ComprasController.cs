using ApiProductos.Data;
using ApiProductos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace ApiProductos.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrador,Inventario")] // ✅ Seguridad global para todo el controlador
    public class ComprasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ComprasController(AppDbContext context)
        {
            _context = context;
        }

        // ========================
        // 📦 GET: api/compras
        // ========================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetCompras([FromQuery] int? sucursalId = null)
        {
            var query = _context.Compras
                .Include(c => c.Proveedor)
                .Include(c => c.ReglaPrecio)
                .Include(c => c.Sucursal)
                .Include(c => c.Detalles)
                    .ThenInclude(d => d.Producto)
                .Include(c => c.Detalles)
                    .ThenInclude(d => d.Presentacion)
                        .ThenInclude(p => p.Unidad)
                .Include(c => c.Detalles)
                    .ThenInclude(d => d.Unidad)
                .AsQueryable();

            // 🔹 Filtro por sucursal
            if (sucursalId.HasValue && sucursalId > 0)
                query = query.Where(c => c.SucursalId == sucursalId.Value);

            var compras = await query
                .OrderByDescending(c => c.FechaCompra)
                .Select(c => new
                {
                    c.Id,
                    c.NumeroFactura,
                    Proveedor = c.Proveedor!.Nombre,
                    c.FechaCompra,
                    c.TotalCompra,
                    c.Iva,
                    c.UsuarioId,
                    c.Observaciones,
                    Sucursal = c.Sucursal != null ? c.Sucursal.Nombre : "Sin sucursal",
                    ReglaPrecio = c.ReglaPrecio != null
                        ? c.ReglaPrecio.TipoCliente + $" ({c.ReglaPrecio.Porcentaje}%)"
                        : "Sin regla",
                    Detalles = c.Detalles.Select(d => new
                    {
                        d.Id,
                        d.CodigoAlterno,
                        Producto = d.Producto!.Nombre,
                        Presentacion = d.Presentacion != null ? d.Presentacion.Tipo : "Sin presentación",
                        UnidadBase = d.Presentacion != null && d.Presentacion.Unidad != null
                            ? d.Presentacion.Unidad.Nombre
                            : (d.Unidad != null ? d.Unidad.Nombre : "Sin unidad"),
                        d.Lote,
                        d.Cantidad,
                        d.PrecioCompra,
                        d.PrecioVenta,
                        d.Subtotal
                    }).ToList()
                })
                .ToListAsync();

            return Ok(compras);
        }

        // ========================
        // 🧾 POST: api/compras
        // ========================
        [HttpPost]
        public async Task<ActionResult> RegistrarCompra([FromBody] Compra compra)
        {
            if (compra == null || compra.Detalles == null || !compra.Detalles.Any())
                return BadRequest("❌ La compra debe tener al menos un producto.");

            if (compra.SucursalId <= 0)
                return BadRequest("❌ Debe especificar la sucursal.");

            // ✅ Obtener usuario logueado (opcional, si ya usas JWT)
            int usuarioId = 1; // 🔸 Temporal: reemplazar por claim del token
            decimal margen = 0.25m;

            // 🔹 Buscar regla de precio si aplica
            if (compra.ReglaPrecioId.HasValue)
            {
                var regla = await _context.ReglasPrecio
                    .FirstOrDefaultAsync(r => r.Id == compra.ReglaPrecioId && r.Activo);
                if (regla != null)
                    margen = regla.Porcentaje / 100m;
            }

            compra.FechaCompra = DateTime.Now;
            compra.Iva = 0.12m;
            compra.UsuarioId = usuarioId;

            // 🔹 Procesar cada detalle de compra
            foreach (var d in compra.Detalles)
            {
                decimal factorConversion = 1;
                if (d.PresentacionId.HasValue)
                {
                    var presentacion = await _context.Presentaciones
                        .FirstOrDefaultAsync(p => p.Id == d.PresentacionId);
                    if (presentacion != null && presentacion.FactorConversion > 0)
                        factorConversion = presentacion.FactorConversion;
                }

                if (string.IsNullOrWhiteSpace(d.Lote))
                    d.Lote = $"L{DateTime.Now:yyyyMMddHHmmss}";

                d.Subtotal = d.Cantidad * d.PrecioCompra;

                decimal costoUnitario = d.PrecioCompra / factorConversion;
                decimal precioVentaUnitario = costoUnitario + (costoUnitario * margen);

                // 🔹 Buscar o crear stock
                var stockExistente = await _context.StockProductos
                    .FirstOrDefaultAsync(s =>
                        s.ProductoId == d.ProductoId &&
                        s.ProveedorId == compra.ProveedorId &&
                        s.SucursalId == compra.SucursalId &&
                        (s.CodigoAlterno == d.CodigoAlterno ||
                         (s.CodigoAlterno == null && d.CodigoAlterno == null)));

                if (stockExistente != null)
                {
                    stockExistente.CostoUnitario = (stockExistente.CostoUnitario + costoUnitario) / 2;
                    stockExistente.PrecioVentaUnitario = stockExistente.CostoUnitario + (stockExistente.CostoUnitario * margen);
                    stockExistente.Costo = stockExistente.CostoUnitario;
                    stockExistente.PrecioVenta = stockExistente.PrecioVentaUnitario;
                    stockExistente.StockActual += d.Cantidad * factorConversion;
                    stockExistente.FechaUltimaCompra = DateTime.Now;
                }
                else
                {
                    _context.StockProductos.Add(new StockProducto
                    {
                        ProductoId = d.ProductoId,
                        ProveedorId = compra.ProveedorId,
                        CodigoAlterno = d.CodigoAlterno,
                        Lote = d.Lote,
                        StockActual = d.Cantidad * factorConversion,
                        Costo = costoUnitario,
                        PrecioVenta = precioVentaUnitario,
                        CostoUnitario = costoUnitario,
                        PrecioVentaUnitario = precioVentaUnitario,
                        FechaUltimaCompra = DateTime.Now,
                        Activo = true,
                        SucursalId = compra.SucursalId
                    });
                }

                d.PrecioVenta = d.PrecioCompra + (d.PrecioCompra * margen);
                d.FechaIngreso = compra.FechaCompra;
                d.Observaciones ??= "Ingreso desde módulo de compras";
                d.Compra = compra;
            }

            compra.TotalCompra = compra.Detalles.Sum(d => d.Subtotal);

            await _context.Compras.AddAsync(compra);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "✅ Compra registrada correctamente.",
                compraId = compra.Id,
                sucursal = compra.SucursalId
            });
        }
    }
}