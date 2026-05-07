// Pages/Usuarios.cshtml.cs

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TienditaApp.Models;
using TienditaApp.Repositories;

namespace TienditaApp.Pages
{
    public class UsuariosModel : PageModel
    {
        private readonly UsuarioRepository _usuarioRepo;

        public UsuariosModel(UsuarioRepository usuarioRepo)
        {
            _usuarioRepo = usuarioRepo;
        }

        [BindProperty]
        public Usuario Usuario { get; set; } = new();

        public List<Usuario> ListaUsuarios { get; set; } = new();

        public string Negocio { get; set; } = "";

        public string Rol { get; set; } = "";

        // 🔹 GET
        public IActionResult OnGet()
        {
            // 🔒 Validar sesión
            if (HttpContext.Session.GetString("Usuario") == null)
            {
                return RedirectToPage("/Login");
            }

            // 🔒 Solo ADMIN
            Rol = HttpContext.Session.GetString("Rol") ?? "";

            if (Rol != "ADMIN")
            {
                return RedirectToPage("/Index");
            }

            Negocio =
                HttpContext.Session.GetString("Negocio")
                ?? "Mi Tiendita";

            ListaUsuarios = _usuarioRepo.ObtenerTodos();

            return Page();
        }

        // 🔹 INSERT / UPDATE
        public IActionResult OnPost()
        {
            // 🔒 Validar sesión
            if (HttpContext.Session.GetString("Usuario") == null)
            {
                return RedirectToPage("/Login");
            }

            // 🔒 Solo ADMIN
            Rol = HttpContext.Session.GetString("Rol") ?? "";

            if (Rol != "ADMIN")
            {
                return RedirectToPage("/Index");
            }

            if (!ModelState.IsValid)
            {
                ListaUsuarios = _usuarioRepo.ObtenerTodos();
                return Page();
            }

            // 🔥 Validar usuario repetido
            if (Usuario.Id == 0 &&
                _usuarioRepo.ExisteUsuario(Usuario.UsuarioNombre))
            {
                TempData["Error"] =
                    "El nombre de usuario ya existe";

                ListaUsuarios = _usuarioRepo.ObtenerTodos();

                return Page();
            }

            if (Usuario.Id == 0)
            {
                _usuarioRepo.Agregar(Usuario);

                TempData["Mensaje"] =
                    "Usuario creado correctamente ✅";
            }
            else
            {
                _usuarioRepo.Actualizar(Usuario);

                TempData["Mensaje"] =
                    "Usuario actualizado correctamente ✏️";
            }

            return RedirectToPage();
        }

        // 🔹 EDITAR
        public IActionResult OnGetEditar(int id)
        {
            // 🔒 Validar sesión
            if (HttpContext.Session.GetString("Usuario") == null)
            {
                return RedirectToPage("/Login");
            }

            // 🔒 Solo ADMIN
            Rol = HttpContext.Session.GetString("Rol") ?? "";

            if (Rol != "ADMIN")
            {
                return RedirectToPage("/Index");
            }

            ListaUsuarios = _usuarioRepo.ObtenerTodos();

            Usuario = _usuarioRepo.ObtenerPorId(id)
                ?? new Usuario();

            return Page();
        }

        // 🔹 ELIMINAR
        public IActionResult OnPostEliminar(int id)
        {
            // 🔒 Validar sesión
            if (HttpContext.Session.GetString("Usuario") == null)
            {
                return RedirectToPage("/Login");
            }

            // 🔒 Solo ADMIN
            Rol = HttpContext.Session.GetString("Rol") ?? "";

            if (Rol != "ADMIN")
            {
                return RedirectToPage("/Index");
            }

            // 🚫 Evitar eliminar admin principal
            if (id == 1)
            {
                TempData["Error"] =
                    "No puedes eliminar el administrador principal";

                return RedirectToPage();
            }

            _usuarioRepo.Eliminar(id);

            TempData["Mensaje"] =
                "Usuario eliminado 🗑️";

            return RedirectToPage();
        }
    }
}