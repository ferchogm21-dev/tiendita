using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TienditaApp.Models;
using TienditaApp.Repositories;

namespace TienditaApp.Pages;

public class ClientesModel : PageModel
{
    private readonly ClienteRepository _clienteRepo;

    public ClientesModel(ClienteRepository clienteRepo)
    {
        _clienteRepo = clienteRepo;
    }

    [BindProperty]
    public Cliente Cliente { get; set; } = new();

    public List<Cliente> ListaClientes { get; set; } = new();

    public int PageNumber { get; set; }

    public int TotalPages { get; set; }

    // =========================
    // 🔹 MÉTODO REUTILIZABLE
    // =========================
    private void CargarClientes(int pageNumber)
    {
        int usuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 0;

        string rol = HttpContext.Session.GetString("Rol") ?? "";

        int pageSize = 10;

        PageNumber = pageNumber;

        ListaClientes = _clienteRepo
            .ObtenerPaginados(
                PageNumber,
                pageSize,
                usuarioId,
                rol)
            .ToList();

        var total = _clienteRepo.ObtenerTotal(usuarioId, rol);

        TotalPages = (int)Math.Ceiling(total / (double)pageSize);
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

        CargarClientes(pageNumber);

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

        if (!ModelState.IsValid)
        {
            CargarClientes(pageNumber);
            return Page();
        }

        // 🔥 Asignar usuario actual
        Cliente.UsuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 0;

        if (Cliente.Id == 0)
        {
            _clienteRepo.Agregar(Cliente);

            TempData["Mensaje"] =
                "Cliente guardado correctamente ✅";
        }
        else
        {
            _clienteRepo.Actualizar(Cliente);

            TempData["Mensaje"] =
                "Cliente actualizado correctamente ✏️";
        }

        return RedirectToPage(new { pageNumber });
    }

    // =========================
    // 🔹 EDITAR
    // =========================
    public IActionResult OnGetEditar(int id, int pageNumber = 1)
    {
        if (HttpContext.Session.GetString("Usuario") == null)
        {
            return RedirectToPage("/Login");
        }

        int usuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 0;

        string rol = HttpContext.Session.GetString("Rol") ?? "";

        CargarClientes(pageNumber);

        Cliente = _clienteRepo.ObtenerPorId(
            id,
            usuarioId,
            rol);

        if (Cliente == null || Cliente.Id == 0)
        {
            TempData["Mensaje"] =
                "Cliente no encontrado ❌";

            return RedirectToPage(new { pageNumber });
        }

        return Page();
    }

    // =========================
    // 🔹 ELIMINAR
    // =========================
    public IActionResult OnPostEliminar(int id, int pageNumber = 1)
    {
        if (HttpContext.Session.GetString("Usuario") == null)
        {
            return RedirectToPage("/Login");
        }

        int usuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 0;

        string rol = HttpContext.Session.GetString("Rol") ?? "";

        _clienteRepo.Eliminar(id, usuarioId, rol);

        TempData["Mensaje"] =
            "Cliente eliminado 🗑️";

        return RedirectToPage(new { pageNumber });
    }
}