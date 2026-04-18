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

        // 🔹 GET
        public void OnGet(int? clienteId)
            {
                CargarDeudas();

                if (clienteId.HasValue)
                {
                    using var conn = _context.CreateConnection();

                    ClienteIdActual = clienteId;

                    ClienteNombreActual = conn.QueryFirstOrDefault<string>(
                        "SELECT Nombre FROM Clientes WHERE Id = @Id",
                        new { Id = clienteId }) ?? "";

                    // 🔥 ESTA LÍNEA FALTABA
                    VentasCliente = conn.Query<Venta>(@"
                        SELECT * 
                        FROM Ventas 
                        WHERE ClienteId = @Id AND EsFiado = 1
                    ", new { Id = clienteId }).ToList();
                }
            }

        // 🔥 ABONAR
        public IActionResult OnPostAbonar(int clienteId, decimal abono)
        {
            using var conn = _context.CreateConnection();

            if (abono <= 0)
                return RedirectToPage(new { clienteId });

            var ventas = conn.Query<Venta>(@"
                SELECT * FROM Ventas 
                WHERE ClienteId = @ClienteId AND EsFiado = 1
                ORDER BY Id
            ", new { ClienteId = clienteId }).ToList();

            foreach (var v in ventas)
            {
                var pendiente = v.Total - v.Pagado;

                if (pendiente <= 0) continue;

                var abonoAplicado = Math.Min(abono, pendiente);

                conn.Execute(@"
                    UPDATE Ventas
                    SET Pagado = Pagado + @Abono
                    WHERE Id = @Id
                ", new { Abono = abonoAplicado, Id = v.Id });

                abono -= abonoAplicado;

                if (abono <= 0) break;
            }

            return RedirectToPage(new { clienteId });
        }

        // 🔥 LIQUIDAR TODO
        public IActionResult OnPostLiquidarCliente(int clienteId)
        {
            using var conn = _context.CreateConnection();

            conn.Execute(@"
            UPDATE Ventas
            SET Pagado = Total
            WHERE ClienteId = @ClienteId AND EsFiado = 1
        ", new { ClienteId = clienteId });

            return RedirectToPage(new { clienteId });
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
        }
    }
}