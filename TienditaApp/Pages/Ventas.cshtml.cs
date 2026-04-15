using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TienditaApp.Data;
using TienditaApp.Models;

namespace TienditaApp.Pages
{
    public class VentasModel : PageModel
    {
        private readonly AppDbContext _context;

        public VentasModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public int ProductoId { get; set; }

        [BindProperty]
        public int ClienteId { get; set; }

        [BindProperty]
        public int Cantidad { get; set; }

        [BindProperty]
        public bool EsCredito { get; set; }

        public List<Producto> Productos { get; set; } = new();
        public List<Cliente> Clientes { get; set; } = new();
        public List<Venta> Ventas { get; set; } = new();

        public void OnGet()
        {
            CargarDatos();
        }

        public IActionResult OnPost()
        {
            var producto = _context.Productos.FirstOrDefault(p => p.Id == ProductoId);
            var cliente = _context.Clientes.FirstOrDefault(c => c.Id == ClienteId);

            if (producto != null && cliente != null && producto.Stock >= Cantidad)
            {
                producto.Stock -= Cantidad;

                var venta = new Venta
                {
                    ProductoNombre = producto.Nombre,
                    ClienteNombre = cliente.Nombre,
                    Cantidad = Cantidad,
                    Total = producto.Precio * Cantidad,
                    EsCredito = EsCredito,
                    Pagado = EsCredito ? 0 : producto.Precio * Cantidad,
                    Fecha = DateTime.Now
                };

                _context.Ventas.Add(venta);
                _context.SaveChanges();
            }

            return RedirectToPage();
        }

        private void CargarDatos()
        {
            Productos = _context.Productos.ToList();
            Clientes = _context.Clientes.ToList();
            Ventas = _context.Ventas.ToList();
        }
    }
}