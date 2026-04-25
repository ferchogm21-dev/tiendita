namespace TienditaApp.Models
{
    public class ClienteDeuda
    {
        public int ClienteId { get; set; }
        public string Telefono { get; set; } = "";
        public string ClienteNombre { get; set; } = "";
        public decimal TotalDeuda { get; set; }
        public decimal TotalPagado { get; set; }
        

        public decimal Debe => TotalDeuda - TotalPagado;
    }
}