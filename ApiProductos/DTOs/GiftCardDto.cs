using System;

namespace ApiProductos.DTOs
{
    public class GiftCardCreateDto
    {
        public string Codigo { get; set; } = null!;
        public decimal MontoInicial { get; set; }
        public string Moneda { get; set; } = null!;
        public DateTime FechaExpiracion { get; set; }
    }

    public class GiftCardUpdateDto
    {
        public decimal MontoInicial { get; set; }
        public string Moneda { get; set; } = null!;
        public DateTime FechaExpiracion { get; set; }
        public string Estado { get; set; } = null!; // solo permitir Activa/Anulada manualmente si quieres
    }

    public class GiftCardUseDto
    {
        public decimal Monto { get; set; }
    }
}
