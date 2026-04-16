using Microsoft.AspNetCore.Mvc.RazorPages;
using TienditaApp.Models;
using TienditaApp.Data;
using System.Linq;

namespace TienditaApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public List<Producto> Lista { get; set; } = new(); // 🔥 SOLUCIÓN

        public void OnGet()
        {
            Lista = _context.Productos.ToList();
        }
    }
}