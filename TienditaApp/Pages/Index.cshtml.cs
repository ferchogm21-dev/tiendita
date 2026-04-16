using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TienditaApp.Pages
{
    public class IndexModel : PageModel
    {
        public IActionResult OnGet()
        {
            var usuario = HttpContext.Session.GetString("Usuario");

            if (string.IsNullOrEmpty(usuario))
            {
                return RedirectToPage("/Login");
            }

            return Page();
        }
    }
}