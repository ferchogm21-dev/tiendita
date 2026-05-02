using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;

public class PagosModel : PageModel
{
    public List<DeudaCliente> DeudasCliente { get; set; } = new List<DeudaCliente>();
    public List<Venta> VentasCliente { get; set; } = new List<Venta>();

    public decimal TotalGeneralDeuda { get; set; }

    public int? ClienteIdActual { get; set; }
    public string ClienteNombreActual { get; set; } = "";

    public void OnGet(int? clienteId)
    {
        // 🔹 Cargar clientes con deuda (tu lógica aquí)
        CargarDeudas();

        // 🔥 Si viene cliente, cargar ventas
        if (clienteId != null)
        {
            ClienteIdActual = clienteId;
            VentasCliente = ObtenerVentasCliente(clienteId.Value);

            var cliente = DeudasCliente.FirstOrDefault(c => c.ClienteId == clienteId);
            if (cliente != null)
                ClienteNombreActual = cliente.ClienteNombre;
        }
    }

    private void CargarDeudas()
    {
        using (var conn = new SqliteConnection("Data Source=tienda.db"))
        {
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT c.Id, c.Nombre,
                       IFNULL(SUM(v.Total),0) as Total,
                       IFNULL(SUM(v.Pagado),0) as Pagado
                FROM Clientes c
                LEFT JOIN Ventas v ON c.Id = v.ClienteId
                GROUP BY c.Id, c.Nombre";

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var total = reader.GetDecimal(2);
                    var pagado = reader.GetDecimal(3);
                    var debe = total - pagado;

                    DeudasCliente.Add(new DeudaCliente
                    {
                        ClienteId = reader.GetInt32(0),
                        ClienteNombre = reader.GetString(1),
                        TotalDeuda = total,
                        TotalPagado = pagado,
                        Debe = debe
                    });

                    TotalGeneralDeuda += debe;
                }
            }
        }
    }

    private List<Venta> ObtenerVentasCliente(int clienteId)
    {
        var lista = new List<Venta>();

        using (var conn = new SqliteConnection("Data Source=tienda.db"))
        {
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT v.Id, v.Total, IFNULL(v.Pagado,0), v.Fecha, p.Nombre
                FROM Ventas v
                JOIN Productos p ON v.ProductoId = p.Id
                WHERE v.ClienteId = $clienteId";

            cmd.Parameters.AddWithValue("$clienteId", clienteId);

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    lista.Add(new Venta
                    {
                        Id = reader.GetInt32(0),
                        Total = reader.GetDecimal(1),
                        Pagado = reader.GetDecimal(2),
                        Fecha = reader.GetDateTime(3),
                        ProductoNombre = reader.GetString(4)
                    });
                }
            }
        }

        return lista;
    }
}