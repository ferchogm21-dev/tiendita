using Dapper;
using TienditaApp.Data;
using TienditaApp.Models;

public class ProductoRepository
{
    private readonly DapperContext _context;

    public ProductoRepository(DapperContext context)
    {
        _context = context;
    }

    public IEnumerable<Producto> ObtenerTodos()
    {
        using var connection = _context.CreateConnection();
        return connection.Query<Producto>("SELECT * FROM Productos");
    }

    public void Insertar(Producto p)
    {
        using var connection = _context.CreateConnection();
        var sql = "INSERT INTO Productos (Nombre, Precio, Stock) VALUES (@Nombre, @Precio, @Stock)";
        connection.Execute(sql, p);
    }

    public void Actualizar(Producto p)
    {
        using var connection = _context.CreateConnection();
        var sql = "UPDATE Productos SET Nombre=@Nombre, Precio=@Precio, Stock=@Stock WHERE Id=@Id";
        connection.Execute(sql, p);
    }

    public void Eliminar(int id)
    {
        using var connection = _context.CreateConnection();
        connection.Execute("DELETE FROM Productos WHERE Id=@id", new { id });
    }

    public Producto? ObtenerPorId(int id)
    {
        using var connection = _context.CreateConnection();

        return connection.QueryFirstOrDefault<Producto>(
            "SELECT * FROM Productos WHERE Id = @Id",
            new { Id = id });
    }
}