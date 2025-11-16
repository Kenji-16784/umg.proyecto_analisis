using System;

namespace ApiProductos.Models
{
    public class GiftCard
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = null!;
        public decimal MontoInicial { get; set; }
        public decimal SaldoActual { get; set; }
        public string Moneda { get; set; } = null!;
        public DateTime FechaEmision { get; set; } = DateTime.Now;
        public DateTime FechaExpiracion { get; set; }
        public string Estado { get; set; } = "Activa"; // Activa, Agotada, Expirada, Anulada

        // Auditoría
        public string CreadoPor { get; set; } = null!;
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public string? ModificadoPor { get; set; }
        public DateTime? FechaModificacion { get; set; }
    }
}
