using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TienditaApp.Data;
using TienditaApp.Models;
using System.Linq;

namespace TienditaApp.Pages
{
    public class ClientesModel : PageModel
    {
        private readonly AppDbContext _context;

        public ClientesModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Cliente Cliente { get; set; }

        public List<Cliente> Lista { get; set; }

        public void OnGet()
        {
            Lista = _context.Clientes.ToList();
        }

        public void OnPost()
        {
            if (Cliente != null)
            {
                _context.Clientes.Add(Cliente);
                _context.SaveChanges();
            }

            Lista = _context.Clientes.ToList();
        }
        public IActionResult OnPostEliminar(int id)
{
    var cliente = _context.Clientes.Find(id);
    if (cliente != null)
    {
        _context.Clientes.Remove(cliente);
        _context.SaveChanges();
    }

    return RedirectToPage();
}
    }
}