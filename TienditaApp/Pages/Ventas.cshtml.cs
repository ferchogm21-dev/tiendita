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

    public VentasModel()
    {
        var context = new DapperContext();
        _ventaRepo = new VentaRepository(context);
        _productoRepo = new ProductoRepository(context);
        _clienteRepo = new ClienteRepository(context);
    }

    [BindProperty]
    public Venta Venta { get; set; } = new();

    public List<Producto> Productos { get; set; } = new();
    public List<Cliente> Clientes { get; set; } = new();
    

    public List<VentaDTO> ListaVentas { get; set; } = new();

    public IActionResult OnGet()
    {   
        
        // 👉 TU LÓGICA NORMAL
        ListaVentas = _ventaRepo.ObtenerVentas();
        Productos = _productoRepo.ObtenerTodos().ToList();
        Clientes = _clienteRepo.ObtenerClientes();

        return Page();
    }
    
    public IActionResult OnPost()
    {
        _ventaRepo.RegistrarVenta(Venta);
        return RedirectToPage();
    }
    
}