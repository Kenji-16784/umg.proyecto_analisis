namespace ApiProductos.Models
{
    public class Descuento
    {
        public int Id { get; set; }
        
        // Nombre del tipo de descuento (ej: "Mayorista Premium")
        public string Nombre { get; set; } = null!;

        // Porcentaje de descuento (ej: 15 = 15%)
        public decimal Porcentaje { get; set; }

        // Solo para tener activo/inactivo
        public bool Activo { get; set; } = true;
    }
}
