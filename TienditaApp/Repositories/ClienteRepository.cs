using Dapper;
using TienditaApp.Data;
using TienditaApp.Models;

public class ClienteRepository
{
    private readonly DapperContext _context;

    public ClienteRepository(DapperContext context)
    {
        _context = context;
    }

    public List<Cliente> ObtenerClientes()
    {
        using var connection = _context.CreateConnection();
        return connection.Query<Cliente>("SELECT * FROM Clientes").ToList();
    }

    public void Insertar(Cliente cliente)
    {
        using var connection = _context.CreateConnection();
        connection.Execute(
            "INSERT INTO Clientes (Nombre) VALUES (@Nombre)",
            cliente);
    }
}