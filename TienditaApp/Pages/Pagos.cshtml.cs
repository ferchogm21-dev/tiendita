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
                HAVING SUM(v.Total - v.Pagado) > 0
            ").ToList();
            }
        }

        // 🔥 ABONAR
        public IActionResult OnPostAbonar(int clienteId, decimal abono)
        {
            using var conn = _context.CreateConnection();

            // Validación básica
            if (abono <= 0)
                return RedirectToPage(new { clienteId });

            conn.Execute(@"
                UPDATE Ventas
                SET Pagado = Pagado + @Abono
                WHERE ClienteId = @ClienteId AND EsFiado = 1
            ", new { Abono = abono, ClienteId = clienteId });

            // 🔥 CLAVE: redirigir para recargar datos
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