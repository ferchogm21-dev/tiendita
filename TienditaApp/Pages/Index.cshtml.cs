using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TienditaApp.Pages
{
    public class IndexModel : PageModel
    {
        public string Nombre { get; set; } = "";

        public string Rol { get; set; } = "";

        public string Negocio { get; set; } = "";

        public IActionResult OnGet()
        {
            var usuario =
                HttpContext.Session.GetString("Usuario");

            if (string.IsNullOrEmpty(usuario))
            {
                return RedirectToPage("/Login");
            }

            Nombre =
                HttpContext.Session.GetString("Nombre")
                ?? "";

            Rol =
                HttpContext.Session.GetString("Rol")
                ?? "";

            Negocio =
                HttpContext.Session.GetString("Negocio")
                ?? "Mi Tiendita";

            return Page();
        }
    }
}