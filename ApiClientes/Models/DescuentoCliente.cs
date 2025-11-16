using System.ComponentModel.DataAnnotations;

namespace ApiClientes.Models
{
    public class DescuentoCliente
    {
        public int Id { get; set; }

        [Required]
        public int ReglaPrecioId { get; set; }  // 👈 FK hacia la regla de precio (antes era TipoCliente)

        [Range(0, 100)]
        public decimal Porcentaje { get; set; }

        public bool Activo { get; set; } = true;
    }
}