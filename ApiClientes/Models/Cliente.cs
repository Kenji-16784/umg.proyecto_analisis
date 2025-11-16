using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiClientes.Models
{
    public class Cliente
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(20)]
        public string NIT { get; set; } = "CF";

        [MaxLength(50)]
        public string Telefono { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Direccion { get; set; } = string.Empty;

        public bool Activo { get; set; } = true;

        [Required]
        public int ReglaPrecioId { get; set; }

        // 🔗 Relación con la tabla ReglasPrecio
        [ForeignKey("ReglaPrecioId")]
        public ReglaPrecio? ReglaPrecio { get; set; }
    }
}