using Microsoft.AspNetCore.Mvc.RazorPages;
using TienditaApp.Models;
using TienditaApp.Repositories;

namespace TienditaApp.Pages
{
    public class CatalogoModel : PageModel
    {
        private readonly ProductoRepository _productoRepo;

        public CatalogoModel(
            ProductoRepository productoRepo)
        {
            _productoRepo = productoRepo;
        }

        public List<CatalogoDTO> Productos { get; set; } = new();

        public string Nombrenegocio { get; set; } = "";

        public void OnGet(string nombrenegocio)
        {
            Nombrenegocio = nombrenegocio;

            Productos =
                _productoRepo
                    .ObtenerCatalogoPorSlug(Nombrenegocio);
        }
    }
}