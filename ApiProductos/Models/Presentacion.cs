using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiProductos.Models
{
    public class Presentacion
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int ProductoId { get; set; }

        [Required]
        public int UnidadId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Tipo { get; set; } = string.Empty;

        [Range(0.01, 999999)]
        public decimal FactorConversion { get; set; } = 1;

        public bool Activo { get; set; } = true;

        // Relaciones
        public Producto? Producto { get; set; }
        public Unidad? Unidad { get; set; }
    }
}