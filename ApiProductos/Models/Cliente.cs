// ESTA CLAS LA USE PARA QUE NO DIERA ERROR AL LEER EL CLIENT ENE LA VENTA

using System.ComponentModel.DataAnnotations;

namespace ApiProductos.Models
{
    public class Cliente
    {
        public int Id { get; set; }

        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(20)]
        public string NIT { get; set; } = "CF";
    }
}