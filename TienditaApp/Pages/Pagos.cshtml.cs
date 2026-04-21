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
                    SUM(v.Total) AS TotalDeuda,
                    SUM(v.Pagado) AS TotalPagado
                FROM Ventas v
                LEFT JOIN Clientes c ON c.Id = v.ClienteId
                WHERE v.EsFiado = 1
                GROUP BY v.ClienteId, c.Nombre
            ").ToList();

            // 🔥 AQUÍ VA EL TOTAL GLOBAL (SIEMPRE SE CALCULA)
            TotalGeneralDeuda = conn.ExecuteScalar<decimal>(@"
                SELECT IFNULL(SUM(Total - IFNULL(Pagado,0)), 0)
                FROM Ventas
                WHERE EsFiado = 1
            ");
        }
        public IActionResult OnPostAbonar(int clienteId, decimal abono)
        {
            if (HttpContext.Session.GetString("Usuario") == null)
            {
                return RedirectToPage("/Login");
            }

            using var conn = _context.CreateConnection();

            if (abono <= 0)
                return RedirectToPage(new { clienteId });

            var ventas = conn.Query<Venta>(@"
            SELECT *, IFNULL(Pagado, 0) AS Pagado
            FROM Ventas 
            WHERE ClienteId = @ClienteId AND EsFiado = 1
        ", new { ClienteId = clienteId }).ToList();

            foreach (var v in ventas)
            {
                var pagado = v.Pagado ?? 0;
                var pendiente = v.Total - pagado;

                if (pendiente <= 0) continue;

                var abonoAplicado = Math.Min(abono, pendiente);

                conn.Execute(@"
                    UPDATE Ventas
                    SET Pagado = IFNULL(Pagado, 0) + @Abono
                    WHERE Id = @Id
                ", new { Abono = abonoAplicado, Id = v.Id });

                abono -= abonoAplicado;

                if (abono <= 0) break;
            }

            return RedirectToPage(new { clienteId });
        }
    }
}