using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace ApiUsuarios.Models
{
    public class Rol
    {
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Nombre { get; set; } = string.Empty;

        public bool Activo { get; set; } = true;

        // Relación con usuarios
        public ICollection<Usuario>? Usuarios { get; set; }
    }
}