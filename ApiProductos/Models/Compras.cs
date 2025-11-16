using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiProductos.Models
{
    public class Compra
    {
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string NumeroFactura { get; set; } = string.Empty;

        public int ProveedorId { get; set; }
        public Proveedor? Proveedor { get; set; }

        public DateTime FechaCompra { get; set; } = DateTime.Now;

        [MaxLength(500)]
        public string? Observaciones { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal TotalCompra { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Iva { get; set; }

        public int? UsuarioId { get; set; }

        // 🔹 Nueva relación: regla de precio aplicada
        public int? ReglaPrecioId { get; set; }
        public ReglaPrecio? ReglaPrecio { get; set; }

        // 🔹 Nueva relación: sucursal
        public int SucursalId { get; set; }
        public Sucursal? Sucursal { get; set; }

        public List<CompraDetalle> Detalles { get; set; } = new();
    }
}