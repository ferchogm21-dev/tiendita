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
        public int VentaId { get; set; }

        [BindProperty]
        public decimal Monto { get; set; }

        public List<Venta> Ventas { get; set; }

        public void OnGet()
        {
            Ventas = _context.Ventas
                .Where(v => v.EsCredito && v.Pagado < v.Total)
                .ToList();
        }

        public void OnPost()
        {
            var venta = _context.Ventas.FirstOrDefault(v => v.Id == VentaId);

            if (venta != null)
            {
                venta.Pagado += Monto;
                _context.SaveChanges();
            }

            Ventas = _context.Ventas
                .Where(v => v.EsCredito && v.Pagado < v.Total)
                .ToList();
        }
    }
}