using System.ComponentModel.DataAnnotations;

public class Cliente
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [Display(Name = "Nombre del cliente")]
    public string Nombre { get; set; } = "";

    [Phone(ErrorMessage = "Teléfono no válido")]
    [Display(Name = "Teléfono")]
    public string? Telefono { get; set; }
}