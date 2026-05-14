using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TienditaApp.Models;
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

    // =====================================================
    // 🔹 MÉTODO REUTILIZABLE
    // =====================================================
    private void CargarDatos(int pageNumber)
    {
        int usuarioId =
            HttpContext.Session.GetInt32("UsuarioId") ?? 0;

        string rol =
            HttpContext.Session.GetString("Rol") ?? "";

        int pageSize = 10;

        PageNumber = pageNumber;

        // 🔥 PAGINACIÓN
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

        // 🔥 PRODUCTOS
        Productos = _productoRepo
            .ObtenerTodos(usuarioId, rol)
            .ToList();

        // 🔥 CLIENTES
        Clientes = _clienteRepo
            .ObtenerClientes(usuarioId, rol)
            .ToList();
    }

    // =====================================================
    // 🔹 VALIDACIONES
    // =====================================================
    private bool ValidarVenta(int pageNumber)
    {
        if (Venta.ProductoId == 0)
        {
            ModelState.AddModelError(
                "",
                "Selecciona un producto");

            CargarDatos(pageNumber);

            return false;
        }

        if (Venta.ClienteId == 0)
        {
            ModelState.AddModelError(
                "",
                "Selecciona un cliente");

            CargarDatos(pageNumber);

            return false;
        }

        if (Venta.Cantidad <= 0)
        {
            ModelState.AddModelError(
                "",
                "La cantidad debe ser mayor a 0");

            CargarDatos(pageNumber);

            return false;
        }

        return true;
    }

    // =====================================================
    // 🔹 GET
    // =====================================================
    public IActionResult OnGet(int pageNumber = 1)
    {
        if (HttpContext.Session.GetString("Usuario") == null)
        {
            return RedirectToPage("/Login");
        }

        CargarDatos(pageNumber);

        return Page();
    }

    // =====================================================
    // 🔹 REGISTRAR VENTA
    // =====================================================
    public IActionResult OnPost(int pageNumber = 1)
    {
        if (HttpContext.Session.GetString("Usuario") == null)
        {
            return RedirectToPage("/Login");
        }

        if (!ValidarVenta(pageNumber))
        {
            return Page();
        }

        // 🔥 ASIGNAR USUARIO
        Venta.UsuarioId =
            HttpContext.Session.GetInt32("UsuarioId") ?? 0;

        try
        {
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

    // =====================================================
    // 🔹 EDITAR VENTA
    // =====================================================
    public IActionResult OnPostEditar(int pageNumber = 1)
    {
        if (HttpContext.Session.GetString("Usuario") == null)
        {
            return RedirectToPage("/Login");
        }

        if (Venta.Id <= 0)
        {
            ModelState.AddModelError(
                "",
                "Venta inválida");

            CargarDatos(pageNumber);

            return Page();
        }

        if (!ValidarVenta(pageNumber))
        {
            return Page();
        }

        // 🔥 ASIGNAR USUARIO
        Venta.UsuarioId =
            HttpContext.Session.GetInt32("UsuarioId") ?? 0;

        try
        {
            _ventaRepo.ActualizarVenta(Venta);

            TempData["Mensaje"] =
                "Venta actualizada correctamente ✅";
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