namespace ApiProductos.Models
{
    public class Sucursal
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string? Direccion { get; set; }

        public bool Activo { get; set; } = true;
    }
}