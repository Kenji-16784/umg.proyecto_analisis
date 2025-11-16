using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiProductos.Models
{
    public class StockProducto
    {
        public int Id { get; set; }

        // ===== Relaciones =====
        public int ProductoId { get; set; }
        public Producto? Producto { get; set; }

        public int ProveedorId { get; set; }
        public Proveedor? Proveedor { get; set; }

        public int SucursalId { get; set; }
        public Sucursal? Sucursal { get; set; }

        [MaxLength(100)]
        public string? CodigoAlterno { get; set; }

        [MaxLength(50)]
        public string? Lote { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal StockActual { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Costo { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal PrecioVenta { get; set; }

        //acá vamos a agregar los campos para insertarlos al stock
        [Column(TypeName = "decimal(10,2)")]
        public decimal CostoUnitario { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal PrecioVentaUnitario { get; set; }

        public DateTime FechaUltimaCompra { get; set; } = DateTime.Now;

        public bool Activo { get; set; } = true;
    }
}