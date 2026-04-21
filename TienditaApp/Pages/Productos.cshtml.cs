using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TienditaApp.Models;
using TienditaApp.Repositories;

namespace TienditaApp.Pages;

public class ProductosModel : PageModel
{
    private readonly ProductoRepository _productoRepo;

    public ProductosModel(ProductoRepository productoRepo)
    {
        _productoRepo = productoRepo;
    }

    [BindProperty]
    public Producto Producto { get; set; } = new() { Nombre = "" };

    public List<Producto> ListaProductos { get; set; } = new();
    public int PageNumber { get; set; }
    public int TotalPages { get; set; }


    // 🔹 GET
    public IActionResult OnGet(int pageNumber = 1)
    {   
        if (HttpContext.Session.GetString("Usuario") == null)
        {
            return RedirectToPage("/Login");
        }

        ListaProductos = _productoRepo.ObtenerTodos();
            int pageSize = 10;

                    PageNumber = pageNumber;

                    ListaProductos = _productoRepo.ObtenerPaginados(PageNumber, pageSize).ToList();

                    var total = _productoRepo.ObtenerTotal();
                    TotalPages = (int)Math.Ceiling(total / (double)pageSize);

        return Page();
    }

    // 🔹 INSERT / UPDATE
    public IActionResult OnPost()
    {  
         if (HttpContext.Session.GetString("Usuario") == null)
        {
            return RedirectToPage("/Login");
        }
        if (Producto.Stock <= 0)
        {
            ModelState.AddModelError("", "El stock debe ser mayor a 0");
            return Page();
        }
        if (!ModelState.IsValid)
        {
            ListaProductos = _productoRepo.ObtenerTodos();
            return Page();
        }

        if (Producto.Id == 0)
            _productoRepo.Agregar(Producto);
        else
            _productoRepo.Actualizar(Producto);

        TempData["Mensaje"] = Producto.Id == 0 
            ? "Producto guardado correctamente ✅"
            : "Producto actualizado correctamente ✏️";

        return RedirectToPage();
    }

    // 🔹 EDITAR
    public IActionResult OnGetEditar(int id)
    {   
        Producto = _productoRepo.ObtenerPorId(id) ?? new() { Nombre = "" };
        ListaProductos = _productoRepo.ObtenerTodos();
        return Page();
    }

    // 🔹 ELIMINAR
    public IActionResult OnPostEliminar(int id)
    {
        _productoRepo.Eliminar(id);
        return RedirectToPage();
    }
}