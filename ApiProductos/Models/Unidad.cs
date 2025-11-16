namespace ApiProductos.Models
{
    public class Unidad
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Abreviatura { get; set; } = string.Empty;

        // 👇 nuevo campo lógico
        public bool Activo { get; set; } = true;
    }
}
