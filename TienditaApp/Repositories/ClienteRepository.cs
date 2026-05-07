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
    public List<Cliente> ObtenerClientes(int usuarioId, string rol)
    {
        using var connection = _context.CreateConnection();

        string sql = rol == "ADMIN"
            ? "SELECT * FROM Clientes"
            : "SELECT * FROM Clientes WHERE UsuarioId = @UsuarioId";

        return connection.Query<Cliente>(sql, new
        {
            UsuarioId = usuarioId
        }).ToList();
    }

    // 🔹 Obtener por Id
    public Cliente ObtenerPorId(int id, int usuarioId, string rol)
    {
        using var connection = _context.CreateConnection();

        string sql = rol == "ADMIN"
            ? "SELECT * FROM Clientes WHERE Id = @Id"
            : @"SELECT * FROM Clientes
               WHERE Id = @Id
               AND UsuarioId = @UsuarioId";

        return connection.QueryFirstOrDefault<Cliente>(
            sql,
            new
            {
                Id = id,
                UsuarioId = usuarioId
            }) ?? new Cliente();
    }

    // 🔹 Insertar
    public void Agregar(Cliente cliente)
    {
        using var connection = _context.CreateConnection();

        var sql = @"
            INSERT INTO Clientes
            (Nombre, Telefono, UsuarioId)
            VALUES
            (@Nombre, @Telefono, @UsuarioId)
        ";

        connection.Execute(sql, cliente);
    }

    // 🔹 Actualizar
    public void Actualizar(Cliente cliente)
    {
        using var connection = _context.CreateConnection();

        var sql = @"
            UPDATE Clientes
            SET Nombre = @Nombre,
                Telefono = @Telefono
            WHERE Id = @Id
            AND UsuarioId = @UsuarioId
        ";

        connection.Execute(sql, cliente);
    }

    // 🔹 Eliminar
    public void Eliminar(int id, int usuarioId, string rol)
    {
        using var connection = _context.CreateConnection();

        string sql = rol == "ADMIN"
            ? "DELETE FROM Clientes WHERE Id = @Id"
            : @"DELETE FROM Clientes
               WHERE Id = @Id
               AND UsuarioId = @UsuarioId";

        connection.Execute(sql, new
        {
            Id = id,
            UsuarioId = usuarioId
        });
    }

    // 🔹 Obtener paginados
    public IEnumerable<Cliente> ObtenerPaginados(
        int pageNumber,
        int pageSize,
        int usuarioId,
        string rol)
    {
        using var connection = _context.CreateConnection();

        var offset = (pageNumber - 1) * pageSize;

        string sql = rol == "ADMIN"
            ? @"SELECT * FROM Clientes
                ORDER BY Id DESC
                LIMIT @PageSize OFFSET @Offset"
            : @"SELECT * FROM Clientes
                WHERE UsuarioId = @UsuarioId
                ORDER BY Id DESC
                LIMIT @PageSize OFFSET @Offset";

        return connection.Query<Cliente>(sql, new
        {
            UsuarioId = usuarioId,
            PageSize = pageSize,
            Offset = offset
        });
    }

    // 🔹 Obtener total
    public int ObtenerTotal(int usuarioId, string rol)
    {
        using var connection = _context.CreateConnection();

        string sql = rol == "ADMIN"
            ? "SELECT COUNT(*) FROM Clientes"
            : "SELECT COUNT(*) FROM Clientes WHERE UsuarioId = @UsuarioId";

        return connection.ExecuteScalar<int>(sql, new
        {
            UsuarioId = usuarioId
        });
    }
}