using System.ComponentModel.DataAnnotations.Schema;

public class Venta
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }

    public int ProductoId { get; set; }

    public int Cantidad { get; set; }

    public decimal Total { get; set; }

    public DateTime Fecha { get; set; }

    public int ClienteId { get; set; }

    // 🔹 Campo llenado desde JOIN
    public string ClienteNombre { get; set; } = "";

    public bool EsFiado { get; set; }

    public decimal? Pagado { get; set; }

    public DateTime? FechaPago { get; set; }

    // 🔹 Propiedad calculada
    public decimal Saldo => Total - (Pagado ?? 0);

    public string? ProductoNombre { get; set; }
}