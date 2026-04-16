namespace TienditaApp.Models
{
    public class VentaDTO
    {
        public int Id { get; set; }
        public string Producto { get; set; } = "";
        public string Cliente { get; set; } = "";
        public int Cantidad { get; set; }
        public decimal Total { get; set; }
        public int EsFiado { get; set; }
        public string Fecha { get; set; } = "";
    }
}