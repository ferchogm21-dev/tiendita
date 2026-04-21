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


    // 🔹 GET
    public IActionResult OnGet(int pageNumber = 1)
    {
        if (HttpContext.Session.GetString("Usuario") == null)
        {
            return RedirectToPage("/Login");
        }
        ListaClientes = _clienteRepo.ObtenerClientes();
        int pageSize = 10;

        PageNumber = pageNumber;

        ListaClientes = _clienteRepo.ObtenerPaginados(PageNumber, pageSize).ToList();

        var total = _clienteRepo.ObtenerTotal();
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
        if (!ModelState.IsValid)
        {
            ListaClientes = _clienteRepo.ObtenerClientes();
            return Page();
        }

        if (Cliente.Id == 0)
            _clienteRepo.Agregar(Cliente);
        else
            _clienteRepo.Actualizar(Cliente);

        TempData["Mensaje"] = Cliente.Id == 0
            ? "Cliente guardado correctamente ✅"
            : "Cliente actualizado correctamente ✏️";

        return RedirectToPage();
    }

    // 🔹 EDITAR
    public IActionResult OnGetEditar(int id)
    {
        Cliente = _clienteRepo.ObtenerPorId(id);
        ListaClientes = _clienteRepo.ObtenerClientes();
        return Page();
    }

    // 🔹 ELIMINAR
    public IActionResult OnPostEliminar(int id)
    {
        _clienteRepo.Eliminar(id);
        return RedirectToPage();
    }
}