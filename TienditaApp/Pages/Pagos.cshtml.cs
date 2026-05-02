using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Dapper;
using TienditaApp.Data;
using TienditaApp.Models;


namespace TienditaApp.Pages
{
    public class PagosModel : PageModel
    {
        private readonly DapperContext _context;

        public PagosModel(DapperContext context)
        {
            _context = context;
        }

        public List<ClienteDeuda> DeudasCliente { get; set; } = new();
        public List<Venta> VentasCliente { get; set; } = new();

        public int? ClienteIdActual { get; set; }
        public string ClienteNombreActual { get; set; } = "";

        public decimal TotalDeudaCliente { get; set; }
        public decimal TotalGeneralDeuda { get; set; }

        // 🔹 GET
        public IActionResult OnGet(int? clienteId)
        {
            if (HttpContext.Session.GetString("Usuario") == null)
        {
            return RedirectToPage("/Login");
        }
            CargarDeudas(); // 🔥 SIEMPRE se ejecuta
            
            if (clienteId.HasValue)
            {
                using var conn = _context.CreateConnection();

                ClienteIdActual = clienteId;

                ClienteNombreActual = conn.QueryFirstOrDefault<string>(
                    "SELECT Nombre FROM Clientes WHERE Id = @Id",
                    new { Id = clienteId }) ?? "";

                VentasCliente = conn.Query<Venta>(@"
                    SELECT 
                        v.*,
                        p.Nombre AS ProductoNombre
                    FROM Ventas v
                    LEFT JOIN Productos p ON p.Id = v.ProductoId
                    WHERE v.ClienteId = @Id AND v.EsFiado = 1
                ", new { Id = clienteId }).ToList();

                TotalDeudaCliente = conn.ExecuteScalar<decimal>(@"
                    SELECT IFNULL(SUM(v.Total - IFNULL(v.Pagado, 0)), 0)
                    FROM Ventas v
                    WHERE v.ClienteId = @Id AND v.EsFiado = 1
                ", new { Id = clienteId });
            }

            return Page();
        }

        // 🔥 LIQUIDAR TODO
        public IActionResult OnPostLiquidarCliente(int clienteId)
        {
            using var conn = _context.CreateConnection();

            var deuda = conn.ExecuteScalar<decimal>(@"
                SELECT SUM(Total - Pagado)
                FROM Ventas
                WHERE ClienteId = @ClienteId AND EsFiado = 1
            ", new { ClienteId = clienteId });

            // 🔥 si aún debe, NO dejar liquidar
            if (deuda > 0)
            {
                TempData["Error"] = "El cliente aún tiene saldo pendiente.";
                return RedirectToPage(new { clienteId });
            }

            conn.Execute(@"
                UPDATE Ventas
                SET Pagado = Total
                WHERE ClienteId = @ClienteId AND EsFiado = 1
            ", new { ClienteId = clienteId });

            return RedirectToPage();
        }

        // 🔹 Cargar deudas
        private void CargarDeudas()
        {
            using var conn = _context.CreateConnection();

            DeudasCliente = conn.Query<ClienteDeuda>(@"
            SELECT 
                v.ClienteId,
                c.Nombre AS ClienteNombre,
                c.Telefono,
                SUM(v.Total) AS TotalDeuda,
                SUM(v.Pagado) AS TotalPagado
            FROM Ventas v
            LEFT JOIN Clientes c ON c.Id = v.ClienteId
            WHERE v.EsFiado = 1
            GROUP BY v.ClienteId, c.Nombre, c.Telefono
        ").ToList();

            // 🔥 AQUÍ VA EL TOTAL GLOBAL (SIEMPRE SE CALCULA)
            TotalGeneralDeuda = conn.ExecuteScalar<decimal>(@"
                SELECT IFNULL(SUM(Total - IFNULL(Pagado,0)), 0)
                FROM Ventas
                WHERE EsFiado = 1
            ");
        }
        public IActionResult OnPostAbonarVenta(int ventaId, int clienteId, decimal abono)
        {
            if (HttpContext.Session.GetString("Usuario") == null)
                return RedirectToPage("/Login");

            if (abono <= 0)
            {
                TempData["Error"] = "El abono debe ser mayor a 0";
                return RedirectToPage(new { clienteId });
            }

            using var conn = _context.CreateConnection();

            var venta = conn.QueryFirstOrDefault<Venta>(@"
                SELECT *, IFNULL(Pagado, 0) AS Pagado
                FROM Ventas
                WHERE Id = @Id
            ", new { Id = ventaId });

            if (venta == null)
                return RedirectToPage(new { clienteId });

            var pagado = venta.Pagado ?? 0;
            var pendiente = venta.Total - pagado;

            if (pendiente <= 0)
                return RedirectToPage(new { clienteId });

            var abonoAplicado = Math.Min(abono, pendiente);

            conn.Execute(@"
                UPDATE Ventas
                SET Pagado = IFNULL(Pagado, 0) + @Abono
                WHERE Id = @Id
            ", new { Abono = abonoAplicado, Id = ventaId });

            return RedirectToPage(new { clienteId });
        }

        // ✔ LIQUIDAR PRODUCTO
        public IActionResult OnPostLiquidarVenta(int ventaId, int clienteId)
        {
            if (HttpContext.Session.GetString("Usuario") == null)
                return RedirectToPage("/Login");

            using var conn = _context.CreateConnection();

            conn.Execute(@"
                UPDATE Ventas
                SET Pagado = Total
                WHERE Id = @Id
            ", new { Id = ventaId });

            return RedirectToPage(new { clienteId });
        }

        // ↩ DESHACER LIQUIDACIÓN
        public IActionResult OnPostDeshacerLiquidacion(int ventaId, int clienteId)
        {
            if (HttpContext.Session.GetString("Usuario") == null)
                return RedirectToPage("/Login");

            using var conn = _context.CreateConnection();

            conn.Execute(@"
                UPDATE Ventas
                SET Pagado = 0
                WHERE Id = @Id
            ", new { Id = ventaId });

            return RedirectToPage(new { clienteId });
        }
        // 📲 GENERAR MENSAJE DETALLADO WHATSAPP
        public string GenerarMensajeDetalle(int clienteId, string nombre)
        {
            using var conn = _context.CreateConnection();

            var ventas = conn.Query<Venta>(@"
                SELECT v.*, p.Nombre AS ProductoNombre
                FROM Ventas v
                LEFT JOIN Productos p ON p.Id = v.ProductoId
                WHERE v.ClienteId = @Id AND v.EsFiado = 1
            ", new { Id = clienteId }).ToList();

            // 🏪 ENCABEZADO
            var mensaje = "---- Ferxxito ----%0A";
            mensaje += "--------------------%0A";
            mensaje += "Cuenta de Deposito%0A";
            mensaje += "Banco BBVA%0A";
            mensaje += "N° de Tarjeta 4152 3138 8503 9920%0A%0A";
            mensaje += $"Hola {nombre} %0A";
            mensaje += "Te comparto el detalle de tus compras:%0A%0A";

            decimal total = 0;

            foreach (var v in ventas)
            {
                var pagado = v.Pagado ?? 0;
                var pendiente = v.Total - pagado;

                if (pendiente <= 0) continue;

                mensaje += $" {v.ProductoNombre}: ${pendiente}%0A";
                total += pendiente;
            }

            // 💰 TOTAL
            mensaje += $"%0ATotal: ${total} %0A";
            mensaje += "Gracias por tu preferencia ";

            return mensaje;
        }
    }
}