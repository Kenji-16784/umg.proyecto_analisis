using System.ComponentModel.DataAnnotations;

namespace ApiProductos.Models
{
    public class DescuentoCliente
    {
        public int Id { get; set; }

        [Required]
        public TipoCliente TipoCliente { get; set; } // enum ahora dentro de ApiProductos

        [Range(0, 100)]
        public decimal Porcentaje { get; set; }

        public bool Activo { get; set; } = true;
    }
}
