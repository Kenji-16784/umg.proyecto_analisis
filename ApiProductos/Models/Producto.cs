using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ApiProductos.Models
{
    public class Producto
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Descripcion { get; set; }

        public bool Activo { get; set; } = true;

        // 🔗 Relación con Presentaciones
        public List<Presentacion>? Presentaciones { get; set; }
    }
}