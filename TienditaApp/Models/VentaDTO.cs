namespace TienditaApp.Models
{
    public class VentaDTO
    {
        public int Id { get; set; }

        public string Producto { get; set; } = "";

        public string Cliente { get; set; } = "";

        public int Cantidad { get; set; }

        public decimal Total { get; set; }

        public bool EsFiado { get; set; }

        public DateTime Fecha { get; set; }

        public decimal Pagado { get; set; }

        public decimal Saldo { get; set; }
    }
}