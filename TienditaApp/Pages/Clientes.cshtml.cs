using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TienditaApp.Data;
using TienditaApp.Models;

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
        public Cliente Cliente { get; set; } = new Cliente();

        public List<Cliente> Lista { get; set; } = new();

        public void OnGet(int? id)
        {
            if (id.HasValue)
            {
                var cliente = _context.Clientes.FirstOrDefault(x => x.Id == id.Value);

                if (cliente != null)
                {
                    Cliente = cliente;
                }
            }

            Lista = _context.Clientes.ToList();
        }

        // 🟢 CREAR / EDITAR
        public IActionResult OnPost()
        {
            if (Cliente == null)
            {
                return RedirectToPage();
            }

            if (Cliente.Id > 0)
            {
                // ✏️ EDITAR
                var clienteDb = _context.Clientes.FirstOrDefault(x => x.Id == Cliente.Id);

                if (clienteDb != null)
                {
                    clienteDb.Nombre = Cliente.Nombre;
                    clienteDb.Telefono = Cliente.Telefono;
                    _context.SaveChanges();
                }
            }
            else
            {
                // ➕ CREAR
                _context.Clientes.Add(Cliente);
                _context.SaveChanges();
            }

            return RedirectToPage();
        }

        // 🗑 ELIMINAR
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