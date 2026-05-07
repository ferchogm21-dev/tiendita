namespace TienditaApp.Models
{
    public class Usuario
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = "";

        public string UsuarioNombre { get; set; } = "";

        public string Password { get; set; } = "";

        public string Rol { get; set; } = "";
    }
}