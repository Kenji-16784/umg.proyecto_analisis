using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiProductos.Data;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace ApiProductos.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrador,Inventario,Cajero")] // ✅ Acceso general (filtraremos en cada método)
    public class ReportesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReportesController(AppDbContext context)
        {
            _context = context;
        }

        // =======================================
        // 💰 CIERRE DE CAJA
        // =======================================
        [HttpGet("cierre-caja")]
        [Authorize(Roles = "Administrador,Cajero")]
        public async Task<IActionResult> CierreCaja(
            [FromQuery] string? fechaInicio,
            [FromQuery] string? fechaFin,
            [FromQuery] int? sucursalId)
        {
            try
            {
                DateTime? fechaInicioDt = null;
                DateTime? fechaFinDt = null;

                if (DateTime.TryParse(fechaInicio, out var fi))
                    fechaInicioDt = fi.Date;

                if (DateTime.TryParse(fechaFin, out var ff))
                    fechaFinDt = ff.Date.AddDays(1).AddTicks(-1);

                var query = _context.Ventas
                    .Include(v => v.Sucursal)
                    .AsQueryable();

                if (fechaInicioDt.HasValue)
                    query = query.Where(v => v.FechaVenta >= fechaInicioDt.Value);
                if (fechaFinDt.HasValue)
                    query = query.Where(v => v.FechaVenta <= fechaFinDt.Value);
                if (sucursalId.HasValue)
                    query = query.Where(v => v.SucursalId == sucursalId.Value);

                // 🔹 Filtro por usuario (solo si no es admin)
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (userRole?.ToLower() != "administrador" && int.TryParse(userIdClaim, out int usuarioId))
                    query = query.Where(v => v.UsuarioId == usuarioId);

                var ventas = await query.ToListAsync();

                if (!ventas.Any())
                    return Ok(new { mensaje = "No hay ventas registradas en el rango seleccionado." });

                var porMetodoPago = ventas
                    .GroupBy(v => v.MetodoPago)
                    .Select(g => new
                    {
                        MetodoPago = g.Key ?? "No especificado",
                        Total = g.Sum(v => v.Total),
                        Ventas = g.Count()
                    })
                    .ToList();

                var resumen = new
                {
                    FechaInicio = fechaInicioDt?.ToString("yyyy-MM-dd HH:mm") ?? "N/A",
                    FechaFin = fechaFinDt?.ToString("yyyy-MM-dd HH:mm") ?? "N/A",
                    Sucursal = ventas.First().Sucursal?.Nombre ?? "General",
                    TotalVentas = ventas.Count,
                    Subtotal = ventas.Sum(v => v.Subtotal),
                    IVA = ventas.Sum(v => v.Iva),
                    TotalGeneral = ventas.Sum(v => v.Total),
                    PorMetodoPago = porMetodoPago
                };

                return Ok(resumen);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error al generar el cierre de caja", detalle = ex.Message });
            }
        }

        // =======================================
        // 📋 REPORTE DE VENTAS
        // =======================================
        [HttpGet("ventas")]
        [Authorize(Roles = "Administrador,Inventario")]
        public async Task<IActionResult> ReporteVentas(
            [FromQuery] DateTime? fechaInicio,
            [FromQuery] DateTime? fechaFin,
            [FromQuery] int? sucursalId,
            [FromQuery] string? usuario,
            [FromQuery] string? cliente)
        {
            try
            {
                var query = _context.Ventas
                    .Include(v => v.Cliente)
                    .Include(v => v.Sucursal)
                    .AsQueryable();

                if (fechaInicio.HasValue)
                    query = query.Where(v => v.FechaVenta >= fechaInicio.Value.Date);

                if (fechaFin.HasValue)
                    query = query.Where(v => v.FechaVenta <= fechaFin.Value.Date.AddDays(1).AddTicks(-1));

                if (sucursalId.HasValue)
                    query = query.Where(v => v.SucursalId == sucursalId.Value);

                if (!string.IsNullOrEmpty(usuario) && int.TryParse(usuario, out var idUsuario))
                    query = query.Where(v => v.UsuarioId == idUsuario);

                if (!string.IsNullOrEmpty(cliente))
                    query = query.Where(v => v.Cliente.Nombre.Contains(cliente));

                var ventas = await query
                    .OrderByDescending(v => v.FechaVenta)
                    .Select(v => new
                    {
                        v.NumeroFactura,
                        Fecha = v.FechaVenta,
                        Cliente = v.Cliente.Nombre,
                        Sucursal = v.Sucursal.Nombre,
                        v.MetodoPago,
                        v.Subtotal,
                        v.Iva,
                        v.Total
                    })
                    .ToListAsync();

                if (!ventas.Any())
                    return Ok(new { mensaje = "No hay ventas en el rango seleccionado." });

                var resumen = new
                {
                    TotalVentas = ventas.Count,
                    TotalSubtotal = ventas.Sum(v => v.Subtotal),
                    TotalIVA = ventas.Sum(v => v.Iva),
                    TotalGeneral = ventas.Sum(v => v.Total)
                };

                return Ok(new { ventas, resumen });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error al generar el reporte", detalle = ex.Message });
            }
        }

        // =======================================
        // 📦 REPORTE DE STOCK ACTUAL
        // =======================================
        [HttpGet("stock-actual")]
        [Authorize(Roles = "Administrador,Inventario,Cajero")]
        public async Task<IActionResult> ReporteStockActual([FromQuery] int? sucursalId)
        {
            try
            {
                var query = _context.StockProductos
                    .Include(s => s.Producto)
                        .ThenInclude(p => p.Presentaciones)
                            .ThenInclude(pr => pr.Unidad)
                    .Include(s => s.Proveedor)
                    .Include(s => s.Sucursal)
                    .AsNoTracking()
                    .AsQueryable();

                if (sucursalId.HasValue && sucursalId > 0)
                    query = query.Where(s => s.SucursalId == sucursalId.Value);

                var data = await query
                    .Select(s => new
                    {
                        Sucursal = s.Sucursal != null ? s.Sucursal.Nombre : "Sin sucursal",
                        Producto = s.Producto != null ? s.Producto.Nombre : "Sin producto",
                        Proveedor = s.Proveedor != null ? s.Proveedor.Nombre : "Sin proveedor",
                        CodigoAlterno = s.CodigoAlterno ?? "—",
                        UnidadBase = s.Producto!.Presentaciones.FirstOrDefault() != null ?
                            s.Producto.Presentaciones.FirstOrDefault()!.Unidad.Nombre :
                            "—",
                        StockActual = Math.Round(s.StockActual, 2),
                        PrecioVenta = Math.Round(s.PrecioVenta, 2),
                        FechaUltimaCompra = s.FechaUltimaCompra != DateTime.MinValue
                            ? s.FechaUltimaCompra
                            : (DateTime?)null
                    })
                    .OrderBy(x => x.Sucursal)
                    .ThenBy(x => x.Producto)
                    .ToListAsync();

                if (!data.Any())
                    return Ok(new { mensaje = "No hay productos con stock registrado." });

                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error al generar el reporte de stock actual.", detalle = ex.Message });
            }
        }
    }
}