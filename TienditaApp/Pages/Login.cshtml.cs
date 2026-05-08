using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;

namespace TienditaApp.Pages
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public string Usuario { get; set; } = "";

        [BindProperty]
        public string Password { get; set; } = "";

        public string Error { get; set; } = "";

        // 🔹 GET
        public IActionResult OnGet()
        {
            if (HttpContext.Session.GetString("Usuario") != null)
            {
                return RedirectToPage("/Index");
            }

            return Page();
        }

        // 🔹 LOGIN
        public IActionResult OnPost()
        {
            using var connection =
                new SqliteConnection("Data Source=tienda.db");

            connection.Open();

            var command = connection.CreateCommand();

            command.CommandText = @"
                SELECT
                    Id,
                    Nombre,
                    Rol,
                    NombreNegocio,
                    NumeroCuenta
                FROM Usuarios
                WHERE Usuario = $user
                AND Password = $pass
            ";

            command.Parameters.AddWithValue(
                "$user",
                Usuario);

            command.Parameters.AddWithValue(
                "$pass",
                Password);

            using var reader = command.ExecuteReader();

            if (reader.Read())
            {
                int idUsuario = reader.GetInt32(0);

                string nombre = reader.IsDBNull(1)
                    ? ""
                    : reader.GetString(1);

                string rol = reader.IsDBNull(2)
                    ? "USER"
                    : reader.GetString(2);

                string negocio = reader.IsDBNull(3)
                    ? "Mi Tiendita"
                    : reader.GetString(3);

                // 🔥 SESIÓN
                HttpContext.Session.SetInt32(
                    "UsuarioId",
                    idUsuario);

                HttpContext.Session.SetString(
                    "Nombre",
                    nombre);

                HttpContext.Session.SetString(
                    "Usuario",
                    Usuario);

                HttpContext.Session.SetString(
                    "Rol",
                    rol);

                HttpContext.Session.SetString(
                    "Negocio",
                    negocio);
                string cuenta = reader.IsDBNull(4)
                ? ""
                : reader.GetString(4);

                HttpContext.Session.SetString(
                    "NumeroCuenta",
                    cuenta);

                return RedirectToPage("/Index");
            }

            Error = "Usuario o contraseña incorrectos";

            return Page();
        }

        // 🔹 LOGOUT
        public IActionResult OnPostLogout()
        {
            HttpContext.Session.Clear();

            return RedirectToPage("/Login");
        }
    }
}