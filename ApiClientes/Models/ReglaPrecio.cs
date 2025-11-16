using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiClientes.Models
{
    [Table("ReglasPrecio")] // 👈 importante: así se llama tu tabla real en SQL
    public class ReglaPrecio
    {
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string TipoCliente { get; set; } = string.Empty; // Cliente Final, Mayorista, Premium...

        [Range(0, 1000)]
        public decimal Porcentaje { get; set; } // margen o descuento

        public bool Activo { get; set; } = true;
    }
}