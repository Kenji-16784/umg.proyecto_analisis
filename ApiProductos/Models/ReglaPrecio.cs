using System;
using System.ComponentModel.DataAnnotations;

namespace ApiProductos.Models
{
    public class ReglaPrecio
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string TipoCliente { get; set; } = string.Empty;  // Normal, Mayorista, Premium, etc.

        [Required]
        [Range(-100, 1000, ErrorMessage = "El porcentaje debe estar entre -100 y 1000.")]
        public decimal Porcentaje { get; set; }  // % margen o descuento

        public bool EsPromocion { get; set; } = false;

        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }

        public bool Activo { get; set; } = true;

    }
}
