using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiProductos.Models
{
    public class MovimientoStock
    {
        public int Id { get; set; }

        [Required]
        public int ProductoId { get; set; }
        public Producto? Producto { get; set; }

        [Required]
        [Column(TypeName = "decimal(12,2)")]
        public decimal Cantidad { get; set; }

        [Required, MaxLength(20)]
        public string Tipo { get; set; } = string.Empty; // Compra / Venta / Ajuste

        public DateTime Fecha { get; set; } = DateTime.Now;

        [Required]
        public int SucursalId { get; set; }
        public Sucursal? Sucursal { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal PrecioUnitario { get; set; }

        [MaxLength(500)]
        public string? Observaciones { get; set; }
    }
}