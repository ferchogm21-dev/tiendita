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

            // 🔥 Buscar producto por ID
            var producto = connection.QueryFirstOrDefault<Producto>(
            "SELECT * FROM Productos WHERE Id = @Id",
            new { Id = venta.ProductoId });

            if (producto == null)
                throw new Exception("Producto no encontrado");

            if (producto.Stock < venta.Cantidad)
                throw new Exception("Stock insuficiente");

            if (venta.Cantidad > producto.Stock)
                throw new Exception("Stock insuficiente");

            venta.Total = producto.Precio * venta.Cantidad;

            // 🔥 Manejo fiado
            if (venta.EsFiado)
            {
                venta.Pagado = 0;
            }
            else
            {
                venta.Pagado = venta.Total;
            }

            venta.Fecha = DateTime.Now;

            connection.Execute(@"
            INSERT INTO Ventas 
            (ProductoId, ClienteId, Cantidad, Total, EsFiado, Pagado, Fecha)
            VALUES 
            (@ProductoId, @ClienteId, @Cantidad, @Total, @EsFiado, @Pagado, @Fecha)
            ", venta);

            // 🔥 Descontar stock usando el ID real del producto
            connection.Execute(@"
            UPDATE Productos 
            SET Stock = Stock - @Cantidad 
            WHERE Id = @Id AND Stock >= @Cantidad
            ", new { Cantidad = venta.Cantidad, Id = producto.Id });
        }

        public List<VentaDTO> ObtenerVentas()
        {
            using var connection = _context.CreateConnection();

            return connection.Query<VentaDTO>(@"
            SELECT 
                v.Id,
                p.Nombre AS Producto,
                c.Nombre AS Cliente,
                v.Cantidad,
                v.Total,
                v.EsFiado,
                v.Fecha
            FROM Ventas v
            LEFT JOIN Productos p ON p.Id = v.ProductoId
            LEFT JOIN Clientes c ON c.Id = v.ClienteId
            ORDER BY v.Id DESC
            ").ToList();
        }
        public List<ClienteDeuda> ObtenerDeudas()
        {
            using var connection = _context.CreateConnection();

            return connection.Query<ClienteDeuda>(@"
                SELECT 
                    v.ClienteId,
                    c.Nombre AS ClienteNombre,
                    SUM(v.Total) AS TotalDeuda,
                    SUM(v.Pagado) AS TotalPagado
                FROM Ventas v
                LEFT JOIN Clientes c ON c.Id = v.ClienteId
                WHERE v.EsFiado = 1
                GROUP BY v.ClienteId, c.Nombre
            ").ToList();
        }
    }
}