using Dapper;
using TienditaApp.Models;
using TienditaApp.Data;

public class ClienteRepository
{
    private readonly DapperContext _context;

    public ClienteRepository(DapperContext context)
    {
        _context = context;
    }

    // 🔹 Obtener todos
    public List<Cliente> ObtenerClientes()
    {
        using var connection = _context.CreateConnection();
        var sql = "SELECT * FROM Clientes";
        return connection.Query<Cliente>(sql).ToList();
    }

    // 🔹 Obtener por Id
    public Cliente ObtenerPorId(int id)
    {
        using var connection = _context.CreateConnection();
        var sql = "SELECT * FROM Clientes WHERE Id = @Id";
        return connection.QueryFirstOrDefault<Cliente>(sql, new { Id = id }) ?? new Cliente();
    }

    // 🔹 Insertar
    public void Agregar(Cliente cliente)
    {
        using var connection = _context.CreateConnection();
        var sql = @"INSERT INTO Clientes (Nombre, Telefono)
                    VALUES (@Nombre, @Telefono)";
        connection.Execute(sql, cliente);
    }

    // 🔹 Actualizar
    public void Actualizar(Cliente cliente)
    {
        using var connection = _context.CreateConnection();
        var sql = @"UPDATE Clientes 
                    SET Nombre = @Nombre, Telefono = @Telefono
                    WHERE Id = @Id";
        connection.Execute(sql, cliente);
    }

    // 🔹 Eliminar
    public void Eliminar(int id)
    {
        using var connection = _context.CreateConnection();
        var sql = "DELETE FROM Clientes WHERE Id = @Id";
        connection.Execute(sql, new { Id = id });
    }

    public IEnumerable<Cliente> ObtenerPaginados(int pageNumber, int pageSize)
    {
        using var connection = _context.CreateConnection();

        var offset = (pageNumber - 1) * pageSize;

        var sql = @"SELECT * FROM Clientes
                    ORDER BY Id DESC
                    LIMIT @PageSize OFFSET @Offset";

        return connection.Query<Cliente>(sql, new
        {
            PageSize = pageSize,
            Offset = offset
        });
    }

    public int ObtenerTotal()
    {
        using var connection = _context.CreateConnection();
        return connection.ExecuteScalar<int>("SELECT COUNT(*) FROM Clientes");
    }
    
}