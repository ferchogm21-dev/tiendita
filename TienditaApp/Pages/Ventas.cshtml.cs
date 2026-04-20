using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TienditaApp.Models;
using TienditaApp.Data;
using System.Linq;
using TienditaApp.Repositories;

public class VentasModel : PageModel
{
    private readonly VentaRepository _ventaRepo;
    private readonly ProductoRepository _productoRepo;
    private readonly ClienteRepository _clienteRepo;

    public VentasModel(
        VentaRepository ventaRepo,
        ProductoRepository productoRepo,
        ClienteRepository clienteRepo)
    {
        _ventaRepo = ventaRepo;
        _productoRepo = productoRepo;
        _clienteRepo = clienteRepo;
    }

    [BindProperty]
    public Venta Venta { get; set; } = new();

    public List<Producto> Productos { get; set; } = new();
    public List<Cliente> Clientes { get; set; } = new();
    public List<VentaDTO> ListaVentas { get; set; } = new();

    public IActionResult OnGet()
    {
        if (HttpContext.Session.GetString("Usuario") == null)
        {
            return RedirectToPage("/Login");
        }

        ListaVentas = _ventaRepo.ObtenerVentas();
        Productos = _productoRepo.ObtenerTodos().ToList();
        Clientes = _clienteRepo.ObtenerClientes();

        return Page();
    }

    public IActionResult OnPost()
    {   
        if (HttpContext.Session.GetString("Usuario") == null)
        {
            return RedirectToPage("/Login");
        }
        // 🔥 VALIDACIONES
        if (Venta.ProductoId == 0)
        {
            ModelState.AddModelError("", "Selecciona un producto");
            return Page();
        }

        if (Venta.ClienteId == 0)
        {
            ModelState.AddModelError("", "Selecciona un cliente");
            return Page();
        }

        // ✅ Si todo está bien, registra la venta
        _ventaRepo.RegistrarVenta(Venta);

        return RedirectToPage();
    }
    
}