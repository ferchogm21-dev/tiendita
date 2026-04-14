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

        [BindProperty]
        public Producto Producto { get; set; } = new Producto();

        public List<Producto> Lista { get; set; } = new List<Producto>();

        // 🔹 Cargar página
        public void OnGet()
        {
            Lista = _context.Productos.ToList();
        }

        // 🔥 EDITAR (GET)
        public IActionResult OnGetEditar(int id)
{
    var producto = _context.Productos.FirstOrDefault(x => x.Id == id);

    if (producto != null)
    {
        Producto = new Producto
        {
            Id = producto.Id,
            Nombre = producto.Nombre,
            Precio = producto.Precio,
            Stock = producto.Stock
        };
    }

    Lista = _context.Productos.ToList();

    return Page();
}

        // 🔥 GUARDAR / ACTUALIZAR
        public IActionResult OnPost()
        {
            var prod = _context.Productos
                .FirstOrDefault(x => x.Id == Producto.Id);

            if (prod == null)
            {
                _context.Productos.Add(Producto);
            }
            else
            {
                prod.Nombre = Producto.Nombre;
                prod.Precio = Producto.Precio;
                prod.Stock = Producto.Stock;
            }

            _context.SaveChanges();

            return RedirectToPage();
        }

        // 🗑 ELIMINAR
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