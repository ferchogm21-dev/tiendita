using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;

public class PagosModel : PageModel
{
    public List<DeudaCliente> DeudasCliente { get; set; } = new();
    public List<Venta> VentasCliente { get; set; } = new();

    public decimal TotalGeneralDeuda { get; set; }

    public string Negocio { get; set; } = "";

    public int? ClienteIdActual { get; set; }

    public string ClienteNombreActual { get; set; } = "";

    public IActionResult OnGet(int? clienteId)
    {
        // 🔒 Validar sesión
        int usuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 0;

        if (usuarioId == 0)
        {
            return RedirectToPage("/Login");
        }

        string rol = HttpContext.Session.GetString("Rol") ?? "";
        Negocio = HttpContext.Session.GetString("Negocio") ?? "Mi Tiendita";

        // 🔹 Cargar deudas
        CargarDeudas(usuarioId, rol);

        // 🔹 Cargar ventas del cliente
        if (clienteId != null)
        {
            ClienteIdActual = clienteId;

            VentasCliente = ObtenerVentasCliente(
                clienteId.Value,
                usuarioId,
                rol);

            var cliente = DeudasCliente.FirstOrDefault(c => c.ClienteId == clienteId);

            if (cliente != null)
            {
                ClienteNombreActual = cliente.ClienteNombre;
            }
        }

        return Page();
    }

    private void CargarDeudas(int usuarioId, string rol)
    {
        using var conn = new SqliteConnection("Data Source=tienda.db");

        conn.Open();

        var cmd = conn.CreateCommand();

        cmd.CommandText = rol == "ADMIN"
            ? @"
                SELECT
                    c.Id,
                    c.Nombre,
                    IFNULL(SUM(v.Total),0) as Total,
                    IFNULL(SUM(v.Pagado),0) as Pagado
                FROM Clientes c
                LEFT JOIN Ventas v ON c.Id = v.ClienteId
                GROUP BY c.Id, c.Nombre
            "
            : @"
                SELECT
                    c.Id,
                    c.Nombre,
                    IFNULL(SUM(v.Total),0) as Total,
                    IFNULL(SUM(v.Pagado),0) as Pagado
                FROM Clientes c
                LEFT JOIN Ventas v ON c.Id = v.ClienteId
                WHERE c.UsuarioId = $usuarioId
                GROUP BY c.Id, c.Nombre
            ";

        cmd.Parameters.AddWithValue("$usuarioId", usuarioId);

        using var reader = cmd.ExecuteReader();

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

    private List<Venta> ObtenerVentasCliente(
        int clienteId,
        int usuarioId,
        string rol)
    {
        var lista = new List<Venta>();

        using var conn = new SqliteConnection("Data Source=tienda.db");

        conn.Open();

        var cmd = conn.CreateCommand();

        cmd.CommandText = rol == "ADMIN"
            ? @"
                SELECT
                    v.Id,
                    v.Total,
                    IFNULL(v.Pagado,0),
                    v.Fecha,
                    p.Nombre
                FROM Ventas v
                JOIN Productos p ON v.ProductoId = p.Id
                WHERE v.ClienteId = $clienteId
            "
            : @"
                SELECT
                    v.Id,
                    v.Total,
                    IFNULL(v.Pagado,0),
                    v.Fecha,
                    p.Nombre
                FROM Ventas v
                JOIN Productos p ON v.ProductoId = p.Id
                WHERE v.ClienteId = $clienteId
                AND v.UsuarioId = $usuarioId
            ";

        cmd.Parameters.AddWithValue("$clienteId", clienteId);
        cmd.Parameters.AddWithValue("$usuarioId", usuarioId);

        using var reader = cmd.ExecuteReader();

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

        return lista;
    }
}