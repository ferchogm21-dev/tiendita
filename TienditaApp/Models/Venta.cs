using System.ComponentModel.DataAnnotations.Schema;


public class Venta
{
    public int Id { get; set; }

    public string ProductoNombre { get; set; } = "";

    public int Cantidad { get; set; }

    public decimal Total { get; set; }

    public string Fecha { get; set; } = "";

    public int ClienteId { get; set; }

    [NotMapped] // 🔥 ESTA ES LA CLAVE
    public string ClienteNombre { get; set; } = "";

    public int EsFiado { get; set; }

    public decimal Pagado { get; set; }

 
    public string? FechaPago { get; set; }

    [NotMapped] // 🔥 opcional pero recomendado
    public decimal Saldo => Total - Pagado;
}