public class DeudaCliente
{
    public int ClienteId { get; set; }

    public string ClienteNombre { get; set; } = "";

    public decimal TotalDeuda { get; set; }

    public decimal TotalPagado { get; set; }

    public decimal Debe { get; set; }
}