using Dapper;
using TienditaApp.Models;
using TienditaApp.Data;

namespace TienditaApp.Repositories;

public class ProductoRepository
{
    private readonly DapperContext _context;

    public ProductoRepository(DapperContext context)
    {
        _context = context;
    }

    // 🔹 Obtener todos
    public List<Producto> ObtenerTodos(int usuarioId, string rol)
    {
        using var connection = _context.CreateConnection();

        string sql = rol == "ADMIN"
            ? "SELECT * FROM Productos"
            : "SELECT * FROM Productos WHERE UsuarioId = @UsuarioId";

        return connection.Query<Producto>(sql, new
        {
            UsuarioId = usuarioId
        }).ToList();
    }

    // 🔹 Obtener por Id
    public Producto? ObtenerPorId(int id, int usuarioId, string rol)
    {
        using var connection = _context.CreateConnection();

        string sql = rol == "ADMIN"
            ? "SELECT * FROM Productos WHERE Id = @Id"
            : @"SELECT * FROM Productos
               WHERE Id = @Id
               AND UsuarioId = @UsuarioId";

        return connection.QueryFirstOrDefault<Producto>(
            sql,
            new
            {
                Id = id,
                UsuarioId = usuarioId
            });
    }

    // 🔹 Insertar
    public void Agregar(Producto producto)
    {
        using var connection = _context.CreateConnection();

        var sql = @"
            INSERT INTO Productos
            (Nombre, Precio, Stock, UsuarioId)
            VALUES
            (@Nombre, @Precio, @Stock, @UsuarioId)
        ";

        connection.Execute(sql, producto);
    }

    // 🔹 Actualizar
    public void Actualizar(Producto producto)
    {
        using var connection = _context.CreateConnection();

        var sql = @"
            UPDATE Productos
            SET Nombre = @Nombre,
                Precio = @Precio,
                Stock = @Stock
            WHERE Id = @Id
            AND UsuarioId = @UsuarioId
        ";

        connection.Execute(sql, producto);
    }

    // 🔹 Eliminar
    public void Eliminar(int id, int usuarioId, string rol)
    {
        using var connection = _context.CreateConnection();

        string sql = rol == "ADMIN"
            ? "DELETE FROM Productos WHERE Id = @Id"
            : @"DELETE FROM Productos
               WHERE Id = @Id
               AND UsuarioId = @UsuarioId";

        connection.Execute(sql, new
        {
            Id = id,
            UsuarioId = usuarioId
        });
    }

    // 🔹 Obtener paginados
    public IEnumerable<Producto> ObtenerPaginados(
        int pageNumber,
        int pageSize,
        int usuarioId,
        string rol)
    {
        using var connection = _context.CreateConnection();

        var offset = (pageNumber - 1) * pageSize;

        string sql = rol == "ADMIN"
            ? @"SELECT * FROM Productos
                ORDER BY Id DESC
                LIMIT @PageSize OFFSET @Offset"
            : @"SELECT * FROM Productos
                WHERE UsuarioId = @UsuarioId
                ORDER BY Id DESC
                LIMIT @PageSize OFFSET @Offset";

        return connection.Query<Producto>(sql, new
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
            ? "SELECT COUNT(*) FROM Productos"
            : "SELECT COUNT(*) FROM Productos WHERE UsuarioId = @UsuarioId";

        return connection.ExecuteScalar<int>(sql, new
        {
            UsuarioId = usuarioId
        });
    }
}