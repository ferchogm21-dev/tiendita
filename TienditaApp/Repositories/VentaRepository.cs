using Dapper;
using TienditaApp.Data;
using TienditaApp.Models;

namespace TienditaApp.Repositories
{
    public class VentaRepository
    {
        private readonly DapperContext _context;

        public VentaRepository(DapperContext context)
        {
            _context = context;
        }

        public void RegistrarVenta(Venta venta)
        {
            using var connection = _context.CreateConnection();

            // 🔥 Buscar producto por NOMBRE
            var producto = connection.QueryFirstOrDefault<Producto>(
                "SELECT * FROM Productos WHERE Nombre = @Nombre",
                new { Nombre = venta.ProductoNombre });

            if (producto == null)
                throw new Exception("Producto no encontrado");

            if (producto.Stock <= 0)
                throw new Exception("No hay stock disponible");

            if (venta.Cantidad > producto.Stock)
                throw new Exception("Stock insuficiente");

            venta.Total = producto.Precio * venta.Cantidad;

            // 🔥 Manejo fiado
            if (venta.EsFiado == 1)
            {
                venta.Pagado = 0;
            }
            else
            {
                venta.Pagado = venta.Total;
            }

            venta.Fecha = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            connection.Execute(@"
                INSERT INTO Ventas 
                (ProductoNombre, ClienteId, Cantidad, Total, EsFiado, Pagado, Fecha)
                VALUES 
                (@ProductoNombre, @ClienteId, @Cantidad, @Total, @EsFiado, @Pagado, @Fecha)
            ", venta);

            // 🔥 Descontar stock usando el ID real del producto
            connection.Execute(
                "UPDATE Productos SET Stock = Stock - @Cantidad WHERE Id = @Id",
                new { Cantidad = venta.Cantidad, Id = producto.Id });
        }

        public List<VentaDTO> ObtenerVentas()
{
    using var connection = _context.CreateConnection();

    return connection.Query<VentaDTO>(@"
    SELECT 
        v.Id,
        v.ProductoNombre AS Producto,
        c.Nombre AS Cliente,
        v.Cantidad,
        v.Total,
        v.EsFiado,
        v.Fecha
    FROM Ventas v
    LEFT JOIN Clientes c ON c.Id = v.ClienteId
    ORDER BY v.Id DESC
").ToList();
}
    }
}