using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace ApiUsuarios.Models
{
    public class Sucursal
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Direccion { get; set; } = string.Empty;

        public bool Activo { get; set; } = true;

        // Relación con usuarios
        public ICollection<Usuario>? Usuarios { get; set; }
    }
}