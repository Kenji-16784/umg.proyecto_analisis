using ApiProductos.Data;
using ApiProductos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace ApiProductos.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VentasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public VentasController(AppDbContext context)
        {
            _context = context;
        }

        // 🔹 OBTENER TODAS LAS VENTAS
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<object>>> GetVentas()
        {
            var ventas = await _context.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Producto)
                .Include(v => v.Sucursal)
                .OrderByDescending(v => v.FechaVenta)
                .Select(v => new
                {
                    v.Id,
                    v.NumeroFactura,
                    Cliente = v.Cliente != null ? v.Cliente.Nombre : "Sin cliente",
                    v.FechaVenta,
                    v.MetodoPago,
                    Sucursal = v.Sucursal != null ? v.Sucursal.Nombre : "—",
                    v.Subtotal,
                    v.Iva,
                    v.Total,
                    v.Observaciones
                })
                .ToListAsync();

            return Ok(ventas);
        }

        // 🔹 REGISTRAR VENTA CON TRANSACCIÓN SEGURA
[HttpPost]
[Authorize(Roles = "Administrador,Cajero")]
public async Task<ActionResult> RegistrarVenta([FromBody] Venta venta)
{
    if (venta.Detalles == null || !venta.Detalles.Any())
        return BadRequest("❌ La venta debe tener al menos un producto.");

    if (venta.SucursalId <= 0)
        return BadRequest("⚠️ Debe seleccionar una sucursal válida.");

    // 🧠 Obtener información del usuario logueado desde el token JWT
    var userName = User.Identity?.Name ?? "Desconocido";
    var claimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    int usuarioId = 0;
    int.TryParse(claimId, out usuarioId);

    // ⚙️ Asignar correctamente el usuario a la venta
    venta.UsuarioId = usuarioId > 0 ? usuarioId : 1; // fallback a 1 si no existe
    //venta.CreadoPor = userName;
    venta.FechaCreacion = DateTime.Now;
    venta.FechaVenta = DateTime.Now;

    using var transaction = await _context.Database.BeginTransactionAsync();
    try
    {
        // 🔸 1. Validar stock antes de descontar
        foreach (var d in venta.Detalles)
        {
            var stock = await _context.StockProductos
                .Include(s => s.Producto)
                .FirstOrDefaultAsync(s => s.ProductoId == d.ProductoId && s.SucursalId == venta.SucursalId);

            if (stock == null)
                throw new Exception($"❌ No existe stock para el producto ID {d.ProductoId}.");

            if (stock.StockActual < d.Cantidad)
                throw new Exception($"⚠️ Stock insuficiente para {stock.Producto?.Nombre ?? $"Producto ID {d.ProductoId}"}. Disponible: {stock.StockActual}");
        }

        // 🔸 2. Calcular totales
        decimal subtotal = 0m;
        foreach (var d in venta.Detalles)
        {
            var precioSinIva = Math.Round(d.PrecioUnitario / 1.12m, 2);
            var sub = Math.Round(precioSinIva * d.Cantidad, 2);
            d.Subtotal = sub;
            subtotal += sub;
        }

        venta.Subtotal = Math.Round(subtotal, 2);
        venta.Iva = Math.Round(venta.Subtotal * 0.12m, 2);
        venta.Total = Math.Round(venta.Subtotal + venta.Iva, 2);

        // 🔸 3. Descontar stock y registrar movimiento
        foreach (var d in venta.Detalles)
        {
            var stock = await _context.StockProductos
                .FirstOrDefaultAsync(s => s.ProductoId == d.ProductoId && s.SucursalId == venta.SucursalId);

            if (stock != null)
            {
                stock.StockActual -= d.Cantidad;

                var mov = new MovimientoStock
                {
                    ProductoId = d.ProductoId,
                    Cantidad = d.Cantidad,
                    Tipo = "Venta",
                    Fecha = DateTime.Now,
                    SucursalId = venta.SucursalId,
                    PrecioUnitario = d.PrecioUnitario,
                    Observaciones = $"Venta #{venta.NumeroFactura} realizada por {userName}"
                };

                _context.MovimientosStock.Add(mov);
            }
        }

        // 🔸 4. Registrar la venta principal
        _context.Ventas.Add(venta);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return Ok(new
        {
            message = "✅ Venta registrada correctamente.",
            numeroFactura = venta.NumeroFactura,
            subtotal = venta.Subtotal,
            iva = venta.Iva,
            total = venta.Total,
            usuario = userName
        });
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync();
        return BadRequest(new { error = ex.Message });
    }
}
    }
}