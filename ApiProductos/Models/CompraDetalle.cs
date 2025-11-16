using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiProductos.Models
{
    public class CompraDetalle
    {
        public int Id { get; set; }

        // ===== Relaciones =====
        public int CompraId { get; set; }
        public Compra? Compra { get; set; }

        public int ProductoId { get; set; }
        public Producto? Producto { get; set; }

        public int UnidadId { get; set; }
        public Unidad? Unidad { get; set; }

        public int? PresentacionId { get; set; }
        public Presentacion? Presentacion { get; set; }

        // ===== Datos de compra =====
        [MaxLength(100)]
        public string? CodigoAlterno { get; set; }

        [MaxLength(50)]
        public string? Lote { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Cantidad { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal PrecioCompra { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal PrecioVenta { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Subtotal { get; set; }

        // ===== Campos adicionales =====
        public DateTime FechaIngreso { get; set; } = DateTime.Now;

        [MaxLength(500)]
        public string? Observaciones { get; set; }
    }
}