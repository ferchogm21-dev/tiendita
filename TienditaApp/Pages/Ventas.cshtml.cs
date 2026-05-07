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

    public int PageNumber { get; set; }

    public int TotalPages { get; set; }

    // =========================
    // 🔹 MÉTODO REUTILIZABLE
    // =========================
    private void CargarDatos(int pageNumber)
    {
        int usuarioId =
            HttpContext.Session.GetInt32("UsuarioId") ?? 0;

        string rol =
            HttpContext.Session.GetString("Rol") ?? "";

        int pageSize = 10;

        PageNumber = pageNumber;

        // 🔥 PAGINACIÓN REAL
        ListaVentas = _ventaRepo
            .ObtenerVentasPaginadas(
                PageNumber,
                pageSize,
                usuarioId,
                rol);

        var total =
            _ventaRepo.ObtenerTotal(usuarioId, rol);

        TotalPages =
            (int)Math.Ceiling(total / (double)pageSize);

        // 🔥 SOLO PRODUCTOS DEL USUARIO
        Productos = _productoRepo
            .ObtenerTodos(usuarioId, rol)
            .ToList();

        // 🔥 SOLO CLIENTES DEL USUARIO
        Clientes = _clienteRepo
            .ObtenerClientes(usuarioId, rol)
            .ToList();
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

        CargarDatos(pageNumber);

        return Page();
    }

    // =========================
    // 🔹 POST
    // =========================
    public IActionResult OnPost(int pageNumber = 1)
    {
        if (HttpContext.Session.GetString("Usuario") == null)
        {
            return RedirectToPage("/Login");
        }

        // 🔥 VALIDACIONES
        if (Venta.ProductoId == 0)
        {
            ModelState.AddModelError(
                "",
                "Selecciona un producto");

            CargarDatos(pageNumber);

            return Page();
        }

        if (Venta.ClienteId == 0)
        {
            ModelState.AddModelError(
                "",
                "Selecciona un cliente");

            CargarDatos(pageNumber);

            return Page();
        }

        // 🔥 ASIGNAR USUARIO
        Venta.UsuarioId =
            HttpContext.Session.GetInt32("UsuarioId") ?? 0;

        try
        {
            // ✅ Registrar venta
            _ventaRepo.RegistrarVenta(Venta);

            TempData["Mensaje"] =
                "Venta registrada correctamente ✅";
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);

            CargarDatos(pageNumber);

            return Page();
        }

        return RedirectToPage(new { pageNumber });
    }
}