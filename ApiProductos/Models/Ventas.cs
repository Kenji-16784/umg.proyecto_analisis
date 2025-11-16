using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiProductos.Models
{
    public class Venta
    {
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string NumeroFactura { get; set; } = $"V{DateTime.Now:yyyyMMddHHmmss}";

        [Required]
        public int ClienteId { get; set; }
        public Cliente? Cliente { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        public int? ReglaPrecioId { get; set; }
        public ReglaPrecio? ReglaPrecio { get; set; }

        [Required]
        public int SucursalId { get; set; }
        public Sucursal? Sucursal { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal Subtotal { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal Iva { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal Total { get; set; }

        [MaxLength(50)]
        public string MetodoPago { get; set; } = "Efectivo";

        [MaxLength(500)]
        public string? Observaciones { get; set; }

        public DateTime FechaVenta { get; set; } = DateTime.Now;
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        public List<VentaDetalle> Detalles { get; set; } = new();
    }
}