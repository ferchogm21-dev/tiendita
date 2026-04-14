namespace TienditaApp.Models
{
    public class Producto
    {
        public int Id { get; set; }   // 👈 ESTE ES EL FIX

        public string? Nombre { get; set; }
        public decimal Precio { get; set; }
        public int Stock { get; set; }
    }
}