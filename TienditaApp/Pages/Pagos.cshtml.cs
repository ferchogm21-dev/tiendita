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

                VentasCliente = conn.Query<Venta>(
                    "SELECT * FROM Ventas WHERE ClienteId = @Id AND EsFiado = 1",
                    new { Id = clienteId }).ToList();
            }
        }

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