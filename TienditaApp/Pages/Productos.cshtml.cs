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
    public Producto Producto { get; set; } = new()
    {
        Nombre = ""
    };

    public List<Producto> ListaProductos { get; set; } = new();

    public int PageNumber { get; set; }

    public int TotalPages { get; set; }

    // =========================
    // 🔹 MÉTODO REUTILIZABLE
    // =========================
    private void CargarProductos(int pageNumber)
    {
        int usuarioId =
            HttpContext.Session.GetInt32("UsuarioId") ?? 0;

        string rol =
            HttpContext.Session.GetString("Rol") ?? "";

        int pageSize = 10;

        PageNumber = pageNumber;

        ListaProductos = _productoRepo
            .ObtenerPaginados(
                PageNumber,
                pageSize,
                usuarioId,
                rol)
            .ToList();

        var total =
            _productoRepo.ObtenerTotal(usuarioId, rol);

        TotalPages =
            (int)Math.Ceiling(total / (double)pageSize);
    }

    // =========================
    // 🔹 GET
    // =========================
    public IActionResult OnGet(int pageNumber = 1)
    {
        if (HttpContext.Session.GetString("Usuario") == null)
        {
            return RedirectToPage("/Login");
        }

        CargarProductos(pageNumber);

        return Page();
    }

    // =========================
    // 🔹 INSERT / UPDATE
    // =========================
    public IActionResult OnPost(int pageNumber = 1)
    {
        if (HttpContext.Session.GetString("Usuario") == null)
        {
            return RedirectToPage("/Login");
        }

        if (Producto.Stock <= 0)
        {
            ModelState.AddModelError(
                "",
                "El stock debe ser mayor a 0");

            CargarProductos(pageNumber);

            return Page();
        }

        if (!ModelState.IsValid)
        {
            CargarProductos(pageNumber);

            return Page();
        }

        // 🔥 Asignar usuario actual
        Producto.UsuarioId =
            HttpContext.Session.GetInt32("UsuarioId") ?? 0;

        if (Producto.Id == 0)
        {
            _productoRepo.Agregar(Producto);

            TempData["Mensaje"] =
                "Producto guardado correctamente ✅";
        }
        else
        {
            _productoRepo.Actualizar(Producto);

            TempData["Mensaje"] =
                "Producto actualizado correctamente ✏️";
        }

        return RedirectToPage(new { pageNumber });
    }

    // =========================
    // 🔹 EDITAR
    // =========================
    public IActionResult OnGetEditar(
        int id,
        int pageNumber = 1)
    {
        if (HttpContext.Session.GetString("Usuario") == null)
        {
            return RedirectToPage("/Login");
        }

        int usuarioId =
            HttpContext.Session.GetInt32("UsuarioId") ?? 0;

        string rol =
            HttpContext.Session.GetString("Rol") ?? "";

        CargarProductos(pageNumber);

        Producto = _productoRepo.ObtenerPorId(
            id,
            usuarioId,
            rol) ?? new()
        {
            Nombre = ""
        };

        if (Producto.Id == 0)
        {
            TempData["Mensaje"] =
                "Producto no encontrado ❌";

            return RedirectToPage(new { pageNumber });
        }

        return Page();
    }

    // =========================
    // 🔹 ELIMINAR
    // =========================
    public IActionResult OnPostEliminar(
        int id,
        int pageNumber = 1)
    {
        if (HttpContext.Session.GetString("Usuario") == null)
        {
            return RedirectToPage("/Login");
        }

        int usuarioId =
            HttpContext.Session.GetInt32("UsuarioId") ?? 0;

        string rol =
            HttpContext.Session.GetString("Rol") ?? "";

        _productoRepo.Eliminar(
            id,
            usuarioId,
            rol);

        TempData["Mensaje"] =
            "Producto eliminado 🗑️";

        return RedirectToPage(new { pageNumber });
    }
}