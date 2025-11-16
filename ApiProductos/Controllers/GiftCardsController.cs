using System;
using System.Linq;
using System.Threading.Tasks;
using ApiProductos.Data;
using ApiProductos.DTOs;
using ApiProductos.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiProductos.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class GiftCardsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public GiftCardsController(AppDbContext context)
        {
            _context = context;
        }

        private string GetUserName()
        {
            return User.Identity?.Name ?? "sistema";
        }

        // LISTAR
        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            var cards = await _context.GiftCards.ToListAsync();
            return Ok(cards);
        }

        // OBTENER POR ID
        [HttpGet("{id:int}")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var card = await _context.GiftCards.FindAsync(id);
            if (card == null) return NotFound("Tarjeta no encontrada");
            return Ok(card);
        }

        // OBTENER POR CÓDIGO
        [HttpGet("by-code/{codigo}")]
        public async Task<IActionResult> ObtenerPorCodigo(string codigo)
        {
            var card = await _context.GiftCards.FirstOrDefaultAsync(x => x.Codigo == codigo);
            if (card == null) return NotFound("Tarjeta no encontrada");
            return Ok(card);
        }

        // CREAR
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] GiftCardCreateDto dto)
        {
            if (_context.GiftCards.Any(x => x.Codigo == dto.Codigo))
                return BadRequest("El código de la tarjeta ya existe.");

            if (dto.MontoInicial <= 0)
                return BadRequest("El monto inicial debe ser mayor a cero.");

            var card = new GiftCard
            {
                Codigo = dto.Codigo,
                MontoInicial = dto.MontoInicial,
                SaldoActual = dto.MontoInicial,
                Moneda = dto.Moneda,
                FechaEmision = DateTime.Now,
                FechaExpiracion = dto.FechaExpiracion,
                Estado = "Activa",
                CreadoPor = GetUserName(),
                FechaCreacion = DateTime.Now
            };

            _context.GiftCards.Add(card);
            await _context.SaveChangesAsync();

            return Ok(card);
        }

        // MODIFICAR
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Modificar(int id, [FromBody] GiftCardUpdateDto dto)
        {
            var card = await _context.GiftCards.FindAsync(id);
            if (card == null) return NotFound("Tarjeta no encontrada");

            if (card.Estado == "Anulada" || card.Estado == "Expirada")
                return BadRequest("No se puede modificar una tarjeta anulada o expirada.");

            card.MontoInicial = dto.MontoInicial;
            card.Moneda = dto.Moneda;
            card.FechaExpiracion = dto.FechaExpiracion;

            if (!string.IsNullOrWhiteSpace(dto.Estado))
            {
                if (new[] { "Activa", "Anulada", "Agotada", "Expirada" }.Contains(dto.Estado))
                    card.Estado = dto.Estado;
            }

            card.ModificadoPor = GetUserName();
            card.FechaModificacion = DateTime.Now;

            await _context.SaveChangesAsync();
            return Ok(card);
        }

        // ANULAR (DELETE LOGICO)
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Anular(int id)
        {
            var card = await _context.GiftCards.FindAsync(id);
            if (card == null) return NotFound("Tarjeta no encontrada");

            if (card.Estado == "Anulada")
                return BadRequest("La tarjeta ya está anulada.");

            card.Estado = "Anulada";
            card.ModificadoPor = GetUserName();
            card.FechaModificacion = DateTime.Now;

            await _context.SaveChangesAsync();
            return Ok("Tarjeta anulada correctamente.");
        }

        // USAR TARJETA
        [HttpPost("usar/{codigo}")]
        public async Task<IActionResult> Usar(string codigo, [FromBody] GiftCardUseDto dto)
        {
            if (dto.Monto <= 0)
                return BadRequest("El monto debe ser mayor a cero.");

            var card = await _context.GiftCards.FirstOrDefaultAsync(x => x.Codigo == codigo);
            if (card == null) return NotFound("Tarjeta no encontrada");

            // EXPIRACIÓN AUTOMÁTICA
            if (DateTime.Now > card.FechaExpiracion)
            {
                card.Estado = "Expirada";
                card.ModificadoPor = GetUserName();
                card.FechaModificacion = DateTime.Now;
                await _context.SaveChangesAsync();
                return BadRequest("La tarjeta está expirada.");
            }

            // ESTADOS NO VÁLIDOS
            if (card.Estado == "Anulada" || card.Estado == "Expirada")
                return BadRequest("No se puede operar esta tarjeta.");

            // VALIDAR SALDO
            if (card.SaldoActual - dto.Monto < 0)
                return BadRequest("Saldo insuficiente.");

            card.SaldoActual -= dto.Monto;

            if (card.SaldoActual == 0)
                card.Estado = "Agotada";

            card.ModificadoPor = GetUserName();
            card.FechaModificacion = DateTime.Now;

            await _context.SaveChangesAsync();
            return Ok(card);
        }

        // CONSULTAR SALDO
        [HttpGet("saldo/{codigo}")]
        public async Task<IActionResult> ObtenerSaldo(string codigo)
        {
            var card = await _context.GiftCards
                .Where(x => x.Codigo == codigo)
                .Select(x => new
                {
                    x.Codigo,
                    x.SaldoActual,
                    x.Moneda,
                    x.Estado,
                    x.FechaExpiracion
                })
                .FirstOrDefaultAsync();

            if (card == null) return NotFound("Tarjeta no encontrada");
            return Ok(card);
        }
    }
}
