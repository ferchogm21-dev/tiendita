using System;

namespace TienditaApp.Models
{
    public class Venta
    {
        public int Id { get; set; }

        public int ClienteId { get; set; }
        public string? ClienteNombre { get; set; }

        public string? ProductoNombre { get; set; }
        public int Cantidad { get; set; }
        public decimal Total { get; set; }

        public bool EsCredito { get; set; } // 👈 NUEVO
        public decimal Pagado { get; set; } = 0; // 👈 NUEVO

        public DateTime Fecha { get; set; } = DateTime.Now;
        public DateTime? FechaPago { get; set; }
    }
}