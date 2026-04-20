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

        public IActionResult OnPost()
        {
            using var connection = new SqliteConnection("Data Source=tienda.db");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT COUNT(*) 
                FROM Usuarios 
                WHERE Usuario = $user AND Password = $pass
            ";

            command.Parameters.AddWithValue("$user", Usuario);
            command.Parameters.AddWithValue("$pass", Password);

            long result = (long)(command.ExecuteScalar() ?? 0L);

            if (result > 0)
            {
                HttpContext.Session.SetString("Usuario", Usuario);
                return RedirectToPage("/Index");
            }

            Error = "Usuario o contraseña incorrectos";
            return Page();
        }
        public IActionResult OnPostLogout()
        {
            HttpContext.Session.Clear();
            return RedirectToPage("/Login");
        }
    }
}