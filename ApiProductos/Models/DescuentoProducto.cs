using System.ComponentModel.DataAnnotations;

namespace ApiProductos.Models
{
    public class DescuentoProducto
    {
        public int Id { get; set; }

        [Required]
        public int ProductoId { get; set; }

        public Producto Producto { get; set; } = null!;

        [Range(0, 100)]
        public decimal Porcentaje { get; set; }

        public bool Activo { get; set; } = true;

        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
    }
}
