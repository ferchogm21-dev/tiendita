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
        int pageSize = 10;

        PageNumber = pageNumber;

        ListaClientes = _clienteRepo
            .ObtenerPaginados(PageNumber, pageSize)
            .ToList();

        var total = _clienteRepo.ObtenerTotal();
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

        if (Cliente.Id == 0)
        {
            _clienteRepo.Agregar(Cliente);
            TempData["Mensaje"] = "Cliente guardado correctamente ✅";
        }
        else
        {
            _clienteRepo.Actualizar(Cliente);
            TempData["Mensaje"] = "Cliente actualizado correctamente ✏️";
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

        CargarClientes(pageNumber);

        Cliente = _clienteRepo.ObtenerPorId(id);

        if (Cliente == null)
        {
            TempData["Mensaje"] = "Cliente no encontrado ❌";
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

        _clienteRepo.Eliminar(id);

        TempData["Mensaje"] = "Cliente eliminado 🗑️";

        return RedirectToPage(new { pageNumber });
    }
}