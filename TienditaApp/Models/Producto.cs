using System.ComponentModel.DataAnnotations;

public class Producto
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100, ErrorMessage = "Máximo 100 caracteres")]
    public string Nombre { get; set; } = "";

    [Required(ErrorMessage = "El precio es obligatorio")]
    [Range(0.01, 999999, ErrorMessage = "El precio debe ser mayor a 0")]
    public decimal Precio { get; set; }

    [Required(ErrorMessage = "El stock es obligatorio")]
    [Range(0, 100000, ErrorMessage = "El stock no puede ser negativo")]
    public int Stock { get; set; }
}