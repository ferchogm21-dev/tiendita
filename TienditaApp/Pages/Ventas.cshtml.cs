using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TienditaApp.Data;
using TienditaApp.Models;
using System.Linq;

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

        public List<Producto> Productos { get; set; }
        public List<Cliente> Clientes { get; set; }
        public List<Venta> Ventas { get; set; }

        public void OnGet()
        {
            Productos = _context.Productos.ToList();
            Clientes = _context.Clientes.ToList();
            Ventas = _context.Ventas.ToList();
        }

        public void OnPost()
        {
            var producto = _context.Productos.FirstOrDefault(p => p.Id == ProductoId);
            var cliente = _context.Clientes.FirstOrDefault(c => c.Id == ClienteId);

            if (producto != null && cliente != null && producto.Stock >= Cantidad)
            {
                producto.Stock -= Cantidad;

                var venta = new Venta
                {
                    ProductoNombre = producto.Nombre,
                    ClienteId = cliente.Id,
                    ClienteNombre = cliente.Nombre,
                    Cantidad = Cantidad,
                    Total = producto.Precio * Cantidad,
                    EsCredito = EsCredito,
                    Pagado = EsCredito ? 0 : producto.Precio * Cantidad
                };

                _context.Ventas.Add(venta);
                _context.SaveChanges();
            }

            Productos = _context.Productos.ToList();
            Clientes = _context.Clientes.ToList();
            Ventas = _context.Ventas.ToList();
        }
    }
}