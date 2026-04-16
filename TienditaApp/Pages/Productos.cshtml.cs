using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TienditaApp.Data;
using TienditaApp.Models;
using System.Linq;

namespace TienditaApp.Pages
{
    public class ProductosModel : PageModel
    {
        private readonly AppDbContext _context;

        public ProductosModel(AppDbContext context)
        {
            _context = context;
        }

        // 🔥 AQUÍ VA (IMPORTANTE)
        [BindProperty]
        public Producto Producto { get; set; } = new Producto();

        public List<Producto> Lista { get; set; } = new();

        public void OnGet()
        {
            Lista = _context.Productos.ToList();
        }

        public IActionResult OnPost()
        {
            if (Producto.Id == 0)
            {
                _context.Productos.Add(Producto);
            }
            else
            {
                _context.Productos.Update(Producto);
            }

            _context.SaveChanges();
            return RedirectToPage();
        }

        public IActionResult OnGetEditar(int id)
        {
            var producto = _context.Productos.Find(id);

            if (producto != null)
            {
                Producto = producto;
            }

            Lista = _context.Productos.ToList();
            return Page();
        }

        public IActionResult OnPostEliminar(int id)
        {
            var producto = _context.Productos.Find(id);

            if (producto != null)
            {
                _context.Productos.Remove(producto);
                _context.SaveChanges();
            }

            return RedirectToPage();
        }
    }
}