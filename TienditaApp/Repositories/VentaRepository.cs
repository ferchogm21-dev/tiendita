using Dapper;
using TienditaApp.Data;
using TienditaApp.Migrations;
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

        // 🔹 Registrar venta
        public void RegistrarVenta(Venta venta)
        {
            using var connection = _context.CreateConnection();

            // 🔥 Buscar producto SOLO del usuario
            var producto = connection.QueryFirstOrDefault<Producto>(
                @"SELECT * 
                  FROM Productos 
                  WHERE Id = @Id
                  AND UsuarioId = @UsuarioId",
                new
                {
                    Id = venta.ProductoId,
                    UsuarioId = venta.UsuarioId
                }) ?? throw new Exception("Producto no encontrado");

            if (producto.Stock < venta.Cantidad)
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

            // 🔥 Registrar venta
            connection.Execute(@"
                INSERT INTO Ventas
                (
                    ProductoId,
                    ClienteId,
                    Cantidad,
                    Total,
                    EsFiado,
                    Pagado,
                    Fecha,
                    UsuarioId
                )
                VALUES
                (
                    @ProductoId,
                    @ClienteId,
                    @Cantidad,
                    @Total,
                    @EsFiado,
                    @Pagado,
                    @Fecha,
                    @UsuarioId
                )
            ", venta);

            // 🔥 Descontar stock
            connection.Execute(@"
                UPDATE Productos
                SET Stock = Stock - @Cantidad
                WHERE Id = @Id
                AND UsuarioId = @UsuarioId
                AND Stock >= @Cantidad
            ",
            new
            {
                Cantidad = venta.Cantidad,
                Id = producto.Id,
                UsuarioId = venta.UsuarioId
            });
        }

        // 🔹 Obtener ventas
        public List<VentaDTO> ObtenerVentas(int usuarioId, string rol)
        {
            using var connection = _context.CreateConnection();

            string sql = rol == "ADMIN"
                ? @"
                    SELECT
                        v.Id,
                        p.Nombre AS Producto,
                        c.Nombre AS Cliente,
                        v.Cantidad,
                        v.Total,
                        v.EsFiado,
                        v.Fecha,
                        IFNULL(v.Pagado,0) AS Pagado,
                        (v.Total - IFNULL(v.Pagado,0)) AS Saldo
                    FROM Ventas v
                    LEFT JOIN Productos p ON p.Id = v.ProductoId
                    LEFT JOIN Clientes c ON c.Id = v.ClienteId
                    ORDER BY v.Id DESC
                "
                : @"
                    SELECT
                        v.Id,
                        p.Nombre AS Producto,
                        c.Nombre AS Cliente,
                        v.Cantidad,
                        v.Total,
                        v.EsFiado,
                        v.Fecha,
                        IFNULL(v.Pagado,0) AS Pagado,
                        (v.Total - IFNULL(v.Pagado,0)) AS Saldo
                    FROM Ventas v
                    LEFT JOIN Productos p ON p.Id = v.ProductoId
                    LEFT JOIN Clientes c ON c.Id = v.ClienteId
                    WHERE v.UsuarioId = @UsuarioId
                    ORDER BY v.Id DESC
                ";

            return connection.Query<VentaDTO>(sql, new
            {
                UsuarioId = usuarioId
            }).ToList();
        }

        // 🔹 Obtener deudas
        public List<ClienteDeuda> ObtenerDeudas(int usuarioId, string rol)
        {
            using var connection = _context.CreateConnection();

            string sql = rol == "ADMIN"
                ? @"
                    SELECT
                        v.ClienteId,
                        c.Nombre AS ClienteNombre,
                        SUM(v.Total) AS TotalDeuda,
                        SUM(v.Pagado) AS TotalPagado
                    FROM Ventas v
                    LEFT JOIN Clientes c ON c.Id = v.ClienteId
                    WHERE v.EsFiado = 1
                    GROUP BY v.ClienteId, c.Nombre
                "
                : @"
                    SELECT
                        v.ClienteId,
                        c.Nombre AS ClienteNombre,
                        SUM(v.Total) AS TotalDeuda,
                        SUM(v.Pagado) AS TotalPagado
                    FROM Ventas v
                    LEFT JOIN Clientes c ON c.Id = v.ClienteId
                    WHERE v.EsFiado = 1
                    AND v.UsuarioId = @UsuarioId
                    GROUP BY v.ClienteId, c.Nombre
                ";

            return connection.Query<ClienteDeuda>(sql, new
            {
                UsuarioId = usuarioId
            }).ToList();
        }

        // 🔹 Obtener paginados
        public IEnumerable<Venta> ObtenerPaginados(
            int pageNumber,
            int pageSize,
            int usuarioId,
            string rol)
        {
            using var connection = _context.CreateConnection();

            var offset = (pageNumber - 1) * pageSize;

            string sql = rol == "ADMIN"
                ? @"
                    SELECT v.*,
                        c.Nombre AS ClienteNombre,
                        (v.Total - IFNULL(v.Pagado,0)) AS Saldo
                    FROM Ventas v
                    LEFT JOIN Clientes c ON c.Id = v.ClienteId
                    ORDER BY v.Id DESC
                    LIMIT @PageSize OFFSET @Offset
                "
                : @"
                    SELECT v.*,
                        c.Nombre AS ClienteNombre,
                        (v.Total - IFNULL(v.Pagado,0)) AS Saldo
                    FROM Ventas v
                    LEFT JOIN Clientes c ON c.Id = v.ClienteId
                    WHERE v.UsuarioId = @UsuarioId
                    ORDER BY v.Id DESC
                    LIMIT @PageSize OFFSET @Offset
                ";

            return connection.Query<Venta>(sql, new
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
                ? "SELECT COUNT(*) FROM Ventas"
                : "SELECT COUNT(*) FROM Ventas WHERE UsuarioId = @UsuarioId";

            return connection.ExecuteScalar<int>(sql, new
            {
                UsuarioId = usuarioId
            });
        }

        // 🔹 Obtener ventas paginadas DTO
        public List<VentaDTO> ObtenerVentasPaginadas(
            int pageNumber,
            int pageSize,
            int usuarioId,
            string rol)
        {
            using var connection = _context.CreateConnection();

            var offset = (pageNumber - 1) * pageSize;

            string sql = rol == "ADMIN"
                ? @"
                    SELECT
                        v.Id,
                        p.Nombre AS Producto,
                        c.Nombre AS Cliente,
                        v.Cantidad,
                        v.Total,
                        v.EsFiado,
                        v.Fecha,
                        IFNULL(v.Pagado,0) AS Pagado,
                        (v.Total - IFNULL(v.Pagado,0)) AS Saldo
                    FROM Ventas v
                    LEFT JOIN Productos p ON p.Id = v.ProductoId
                    LEFT JOIN Clientes c ON c.Id = v.ClienteId
                    ORDER BY v.Id DESC
                    LIMIT @PageSize OFFSET @Offset
                "
                : @"
                    SELECT
                        v.Id,
                        p.Nombre AS Producto,
                        c.Nombre AS Cliente,
                        v.Cantidad,
                        v.Total,
                        v.EsFiado,
                        v.Fecha,
                        IFNULL(v.Pagado,0) AS Pagado,
                        (v.Total - IFNULL(v.Pagado,0)) AS Saldo
                    FROM Ventas v
                    LEFT JOIN Productos p ON p.Id = v.ProductoId
                    LEFT JOIN Clientes c ON c.Id = v.ClienteId
                    WHERE v.UsuarioId = @UsuarioId
                    ORDER BY v.Id DESC
                    LIMIT @PageSize OFFSET @Offset
                ";

            return connection.Query<VentaDTO>(sql, new
            {
                UsuarioId = usuarioId,
                PageSize = pageSize,
                Offset = offset
            }).ToList();
        }
    }
}