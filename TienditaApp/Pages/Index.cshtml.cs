using Microsoft.AspNetCore.Mvc;
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

        [BindProperty]
        public Producto Producto { get; set; }

        public List<Producto> Lista { get; set; }

        public void OnGet()
        {
            Lista = _context.Productos.ToList();
        }

        public void OnPost()
        {
            if (Producto != null)
            {
                _context.Productos.Add(Producto);
                _context.SaveChanges();
            }

            Lista = _context.Productos.ToList();
        }
    }
}