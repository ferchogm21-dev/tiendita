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

            var producto = connection.QueryFirstOrDefault<Producto>(
                "SELECT * FROM Productos WHERE Id = @Id",
                new { Id = venta.ProductoId });

            if (producto == null)
                throw new Exception("Producto no encontrado");

            // 🔥 VALIDAR STOCK
            if (producto.Stock <= 0)
                throw new Exception("No hay stock disponible");

            if (venta.Cantidad > producto.Stock)
                throw new Exception("Stock insuficiente");

            venta.Total = producto.Precio * venta.Cantidad;

            if (venta.EsFiado == 1)
            {
                venta.Pagado = 0;
                venta.Saldo = venta.Total;
            }
            else
            {
                venta.Pagado = venta.Total;
                venta.Saldo = 0;
            }
            venta.Fecha = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            connection.Execute(@"
                INSERT INTO Ventas 
                (ProductoId, ClienteId, Cantidad, Total, EsFiado, Pagado, Saldo, Fecha)
                VALUES 
                (@ProductoId, @ClienteId, @Cantidad, @Total, @EsFiado, @Pagado, @Saldo, @Fecha)
            ", venta);

            connection.Execute(
                "UPDATE Productos SET Stock = Stock - @Cantidad WHERE Id = @Id",
                new { Cantidad = venta.Cantidad, Id = venta.ProductoId });
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
        v.EsFiado AS EsFiado,
        v.Fecha
            FROM Ventas v
            LEFT JOIN Productos p ON p.Id = v.ProductoId
            LEFT JOIN Clientes c ON c.Id = v.ClienteId
            ORDER BY v.Id DESC
        ").ToList();
}
    }
}