using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TienditaApp.Data;
using TienditaApp.Models;
using System.Linq;

namespace TienditaApp.Pages
{
    public class PagosModel : PageModel
    {
        private readonly AppDbContext _context;

        public PagosModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public int ClienteId { get; set; }

        [BindProperty]
        public decimal Abono { get; set; }

        public List<ClienteDeuda> DeudasCliente { get; set; } = new();
        public List<Venta> VentasCliente { get; set; } = new();

        public string ClienteNombreActual { get; set; } = "";
        public int? ClienteIdActual { get; set; }

        public void OnGet(int? clienteId)
        {
            CargarDeudas();

            if (clienteId.HasValue)
            {
                ClienteIdActual = clienteId;

                ClienteNombreActual = _context.Clientes
                    .Where(c => c.Id == clienteId)
                    .Select(c => c.Nombre)
                    .FirstOrDefault() ?? "";

                VentasCliente = _context.Ventas
                    .Where(v => v.ClienteId == clienteId && v.EsFiado == 1)
                    .ToList();
            }
        }

        public IActionResult OnPostAbonar()
        {
            var ventas = _context.Ventas
                .Where(v => v.ClienteId == ClienteId && v.EsFiado == 1 && v.Pagado < v.Total)
                .OrderBy(v => v.Id)
                .ToList();

            decimal restante = Abono;

            foreach (var v in ventas)
            {
                if (restante <= 0) break;

                decimal deuda = v.Total - v.Pagado;

                if (restante >= deuda)
                {
                    v.Pagado = v.Total;
                    restante -= deuda;
                }
                else
                {
                    v.Pagado += restante;
                    restante = 0;
                }
            }

            _context.SaveChanges();

            return RedirectToPage(new { clienteId = ClienteId });
        }

        public IActionResult OnPostLiquidarCliente(int clienteId, decimal Abono)
        {
            var ventas = _context.Ventas
                .Where(v => v.ClienteId == clienteId && v.EsFiado == 1 && v.Pagado < v.Total)
                .OrderBy(v => v.Id)
                .ToList();

            decimal restante = Abono;

            foreach (var v in ventas)
            {
                if (restante <= 0) break;

                decimal deuda = v.Total - v.Pagado;

                if (restante >= deuda)
                {
                    v.Pagado = v.Total;
                    restante -= deuda;
                }
                else
                {
                    v.Pagado += restante;
                    restante = 0;
                }
            }

            _context.SaveChanges();

            return RedirectToPage(new { clienteId });
        }

        private void CargarDeudas()
        {
            DeudasCliente = _context.Ventas
                .Where(v => v.EsFiado == 1)
                .GroupBy(v => v.ClienteId)
                .Select(g => new ClienteDeuda
                {
                    ClienteId = g.Key,
                    ClienteNombre = _context.Clientes
                        .Where(c => c.Id == g.Key)
                        .Select(c => c.Nombre)
                        .FirstOrDefault() ?? "",
                    TotalDeuda = g.Sum(x => x.Total),
                    TotalPagado = g.Sum(x => x.Pagado)
                })
                .ToList();
        }
    }

    public class ClienteDeuda
    {
        public int ClienteId { get; set; }
        public string ClienteNombre { get; set; } = "";
        public decimal TotalDeuda { get; set; }
        public decimal TotalPagado { get; set; }
        public decimal Debe => TotalDeuda - TotalPagado;
    }
}