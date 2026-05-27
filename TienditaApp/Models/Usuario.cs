namespace TienditaApp.Models
{
    public class Usuario
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = "";

        public string UsuarioNombre { get; set; } = "";

        public string Password { get; set; } = "";

        public string Rol { get; set; } = "USER";

        public string NombreNegocio { get; set; } = "";

        public string NumeroCuenta { get; set; } = "";

        public string WhatsApp { get; set; } = "";
    }
}