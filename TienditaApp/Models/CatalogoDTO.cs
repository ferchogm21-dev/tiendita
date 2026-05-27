namespace TienditaApp.Models
{
    public class CatalogoDTO
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = "";

        public decimal Precio { get; set; }

        public int Stock { get; set; }

        public string WhatsApp { get; set; } = "";

        public string NombreNegocio { get; set; } = "";
    }
}