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
    public List<Producto> ObtenerTodos()
    {
        using var connection = _context.CreateConnection();
        var sql = "SELECT * FROM Productos";
        return connection.Query<Producto>(sql).ToList();
    }

    // 🔹 Obtener por Id
    public Producto? ObtenerPorId(int id)
    {
        using var connection = _context.CreateConnection();
        var sql = "SELECT * FROM Productos WHERE Id = @Id";
        return connection.QueryFirstOrDefault<Producto>(sql, new { Id = id });
    }

    // 🔹 Insertar
    public void Agregar(Producto producto)
    {
        using var connection = _context.CreateConnection();
        var sql = @"INSERT INTO Productos (Nombre, Precio, Stock)
                    VALUES (@Nombre, @Precio, @Stock)";
        connection.Execute(sql, producto);
    }

    // 🔹 Actualizar
    public void Actualizar(Producto producto)
    {
        using var connection = _context.CreateConnection();
        var sql = @"UPDATE Productos 
                    SET Nombre = @Nombre, Precio = @Precio, Stock = @Stock
                    WHERE Id = @Id";
        connection.Execute(sql, producto);
    }

    // 🔹 Eliminar
    public void Eliminar(int id)
    {
        using var connection = _context.CreateConnection();
        var sql = "DELETE FROM Productos WHERE Id = @Id";
        connection.Execute(sql, new { Id = id });
    }
    public IEnumerable<Producto> ObtenerPaginados(int pageNumber, int pageSize)
    {
        using var connection = _context.CreateConnection();

        var offset = (pageNumber - 1) * pageSize;

        var sql = @"SELECT * FROM Productos
                    ORDER BY Id DESC
                    LIMIT @PageSize OFFSET @Offset";

        return connection.Query<Producto>(sql, new
        {
            PageSize = pageSize,
            Offset = offset
        });
    }

    public int ObtenerTotal()
    {
        using var connection = _context.CreateConnection();
        return connection.ExecuteScalar<int>("SELECT COUNT(*) FROM Productos");
    }
}