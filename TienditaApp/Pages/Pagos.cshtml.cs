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

            int usuarioId =
                HttpContext.Session.GetInt32("UsuarioId") ?? 0;

            string rol =
                HttpContext.Session.GetString("Rol") ?? "";

            CargarDeudas(usuarioId, rol);

            if (clienteId.HasValue)
            {
                using var conn = _context.CreateConnection();

                ClienteIdActual = clienteId;

                // 🔥 Nombre cliente
                ClienteNombreActual = rol == "ADMIN"
                    ? conn.QueryFirstOrDefault<string>(
                        @"SELECT Nombre
                          FROM Clientes
                          WHERE Id = @Id",
                        new
                        {
                            Id = clienteId
                        }) ?? ""
                    : conn.QueryFirstOrDefault<string>(
                        @"SELECT Nombre
                          FROM Clientes
                          WHERE Id = @Id
                          AND UsuarioId = @UsuarioId",
                        new
                        {
                            Id = clienteId,
                            UsuarioId = usuarioId
                        }) ?? "";

                // 🔥 Ventas cliente
                VentasCliente = rol == "ADMIN"
                    ? conn.Query<Venta>(@"
                        SELECT
                            v.*,
                            p.Nombre AS ProductoNombre
                        FROM Ventas v
                        LEFT JOIN Productos p
                            ON p.Id = v.ProductoId
                        WHERE v.ClienteId = @Id
                        AND v.EsFiado = 1
                    ",
                    new
                    {
                        Id = clienteId
                    }).ToList()

                    : conn.Query<Venta>(@"
                        SELECT
                            v.*,
                            p.Nombre AS ProductoNombre
                        FROM Ventas v
                        LEFT JOIN Productos p
                            ON p.Id = v.ProductoId
                        WHERE v.ClienteId = @Id
                        AND v.EsFiado = 1
                        AND v.UsuarioId = @UsuarioId
                    ",
                    new
                    {
                        Id = clienteId,
                        UsuarioId = usuarioId
                    }).ToList();

                // 🔥 Total deuda cliente
                TotalDeudaCliente = rol == "ADMIN"
                    ? conn.ExecuteScalar<decimal>(@"
                        SELECT IFNULL(
                            SUM(v.Total - IFNULL(v.Pagado, 0)),
                            0
                        )
                        FROM Ventas v
                        WHERE v.ClienteId = @Id
                        AND v.EsFiado = 1
                    ",
                    new
                    {
                        Id = clienteId
                    })

                    : conn.ExecuteScalar<decimal>(@"
                        SELECT IFNULL(
                            SUM(v.Total - IFNULL(v.Pagado, 0)),
                            0
                        )
                        FROM Ventas v
                        WHERE v.ClienteId = @Id
                        AND v.EsFiado = 1
                        AND v.UsuarioId = @UsuarioId
                    ",
                    new
                    {
                        Id = clienteId,
                        UsuarioId = usuarioId
                    });
            }

            return Page();
        }

        // 🔥 LIQUIDAR TODO
        public IActionResult OnPostLiquidarCliente(int clienteId)
        {
            if (HttpContext.Session.GetString("Usuario") == null)
            {
                return RedirectToPage("/Login");
            }

            int usuarioId =
                HttpContext.Session.GetInt32("UsuarioId") ?? 0;

            string rol =
                HttpContext.Session.GetString("Rol") ?? "";

            using var conn = _context.CreateConnection();

            decimal deuda = rol == "ADMIN"
                ? conn.ExecuteScalar<decimal>(@"
                    SELECT IFNULL(
                        SUM(Total - IFNULL(Pagado,0)),
                        0
                    )
                    FROM Ventas
                    WHERE ClienteId = @ClienteId
                    AND EsFiado = 1
                ",
                new
                {
                    ClienteId = clienteId
                })

                : conn.ExecuteScalar<decimal>(@"
                    SELECT IFNULL(
                        SUM(Total - IFNULL(Pagado,0)),
                        0
                    )
                    FROM Ventas
                    WHERE ClienteId = @ClienteId
                    AND EsFiado = 1
                    AND UsuarioId = @UsuarioId
                ",
                new
                {
                    ClienteId = clienteId,
                    UsuarioId = usuarioId
                });

            if (deuda > 0)
            {
                TempData["Error"] =
                    "El cliente aún tiene saldo pendiente.";

                return RedirectToPage(new { clienteId });
            }

            if (rol == "ADMIN")
            {
                conn.Execute(@"
                    UPDATE Ventas
                    SET Pagado = Total
                    WHERE ClienteId = @ClienteId
                    AND EsFiado = 1
                ",
                new
                {
                    ClienteId = clienteId
                });
            }
            else
            {
                conn.Execute(@"
                    UPDATE Ventas
                    SET Pagado = Total
                    WHERE ClienteId = @ClienteId
                    AND EsFiado = 1
                    AND UsuarioId = @UsuarioId
                ",
                new
                {
                    ClienteId = clienteId,
                    UsuarioId = usuarioId
                });
            }

            return RedirectToPage();
        }

        // 🔹 Cargar deudas
        private void CargarDeudas(int usuarioId, string rol)
        {
            using var conn = _context.CreateConnection();

            DeudasCliente = rol == "ADMIN"
                ? conn.Query<ClienteDeuda>(@"
                    SELECT
                        v.ClienteId,
                        c.Nombre AS ClienteNombre,
                        c.Telefono,
                        SUM(v.Total) AS TotalDeuda,
                        SUM(v.Pagado) AS TotalPagado
                    FROM Ventas v
                    LEFT JOIN Clientes c
                        ON c.Id = v.ClienteId
                    WHERE v.EsFiado = 1
                    GROUP BY
                        v.ClienteId,
                        c.Nombre,
                        c.Telefono
                ").ToList()

                : conn.Query<ClienteDeuda>(@"
                    SELECT
                        v.ClienteId,
                        c.Nombre AS ClienteNombre,
                        c.Telefono,
                        SUM(v.Total) AS TotalDeuda,
                        SUM(v.Pagado) AS TotalPagado
                    FROM Ventas v
                    LEFT JOIN Clientes c
                        ON c.Id = v.ClienteId
                    WHERE v.EsFiado = 1
                    AND v.UsuarioId = @UsuarioId
                    GROUP BY
                        v.ClienteId,
                        c.Nombre,
                        c.Telefono
                ",
                new
                {
                    UsuarioId = usuarioId
                }).ToList();

            TotalGeneralDeuda = rol == "ADMIN"
                ? conn.ExecuteScalar<decimal>(@"
                    SELECT IFNULL(
                        SUM(Total - IFNULL(Pagado,0)),
                        0
                    )
                    FROM Ventas
                    WHERE EsFiado = 1
                ")

                : conn.ExecuteScalar<decimal>(@"
                    SELECT IFNULL(
                        SUM(Total - IFNULL(Pagado,0)),
                        0
                    )
                    FROM Ventas
                    WHERE EsFiado = 1
                    AND UsuarioId = @UsuarioId
                ",
                new
                {
                    UsuarioId = usuarioId
                });
        }

        // 🔹 ABONAR
        public IActionResult OnPostAbonarVenta(
            int ventaId,
            int clienteId,
            decimal abono)
        {
            if (HttpContext.Session.GetString("Usuario") == null)
            {
                return RedirectToPage("/Login");
            }

            int usuarioId =
                HttpContext.Session.GetInt32("UsuarioId") ?? 0;

            string rol =
                HttpContext.Session.GetString("Rol") ?? "";

            if (abono <= 0)
            {
                TempData["Error"] =
                    "El abono debe ser mayor a 0";

                return RedirectToPage(new { clienteId });
            }

            using var conn = _context.CreateConnection();

            var venta = rol == "ADMIN"
                ? conn.QueryFirstOrDefault<Venta>(@"
                    SELECT *,
                    IFNULL(Pagado,0) AS Pagado
                    FROM Ventas
                    WHERE Id = @Id
                ",
                new
                {
                    Id = ventaId
                })

                : conn.QueryFirstOrDefault<Venta>(@"
                    SELECT *,
                    IFNULL(Pagado,0) AS Pagado
                    FROM Ventas
                    WHERE Id = @Id
                    AND UsuarioId = @UsuarioId
                ",
                new
                {
                    Id = ventaId,
                    UsuarioId = usuarioId
                });

            if (venta == null)
            {
                return RedirectToPage(new { clienteId });
            }

            var pagado = venta.Pagado ?? 0;

            var pendiente = venta.Total - pagado;

            if (pendiente <= 0)
            {
                return RedirectToPage(new { clienteId });
            }

            var abonoAplicado =
                Math.Min(abono, pendiente);

            conn.Execute(@"
                UPDATE Ventas
                SET Pagado =
                    IFNULL(Pagado,0) + @Abono
                WHERE Id = @Id
            ",
            new
            {
                Abono = abonoAplicado,
                Id = ventaId
            });

            return RedirectToPage(new { clienteId });
        }

        // ✔ LIQUIDAR PRODUCTO
        public IActionResult OnPostLiquidarVenta(
            int ventaId,
            int clienteId)
        {
            if (HttpContext.Session.GetString("Usuario") == null)
            {
                return RedirectToPage("/Login");
            }

            int usuarioId =
                HttpContext.Session.GetInt32("UsuarioId") ?? 0;

            string rol =
                HttpContext.Session.GetString("Rol") ?? "";

            using var conn = _context.CreateConnection();

            if (rol == "ADMIN")
            {
                conn.Execute(@"
                    UPDATE Ventas
                    SET Pagado = Total
                    WHERE Id = @Id
                ",
                new
                {
                    Id = ventaId
                });
            }
            else
            {
                conn.Execute(@"
                    UPDATE Ventas
                    SET Pagado = Total
                    WHERE Id = @Id
                    AND UsuarioId = @UsuarioId
                ",
                new
                {
                    Id = ventaId,
                    UsuarioId = usuarioId
                });
            }

            return RedirectToPage(new { clienteId });
        }

        // ↩ DESHACER LIQUIDACIÓN
        public IActionResult OnPostDeshacerLiquidacion(
            int ventaId,
            int clienteId)
        {
            if (HttpContext.Session.GetString("Usuario") == null)
            {
                return RedirectToPage("/Login");
            }

            int usuarioId =
                HttpContext.Session.GetInt32("UsuarioId") ?? 0;

            string rol =
                HttpContext.Session.GetString("Rol") ?? "";

            using var conn = _context.CreateConnection();

            if (rol == "ADMIN")
            {
                conn.Execute(@"
                    UPDATE Ventas
                    SET Pagado = 0
                    WHERE Id = @Id
                ",
                new
                {
                    Id = ventaId
                });
            }
            else
            {
                conn.Execute(@"
                    UPDATE Ventas
                    SET Pagado = 0
                    WHERE Id = @Id
                    AND UsuarioId = @UsuarioId
                ",
                new
                {
                    Id = ventaId,
                    UsuarioId = usuarioId
                });
            }

            return RedirectToPage(new { clienteId });
        }

        // 📲 GENERAR MENSAJE WHATSAPP
        public string GenerarMensajeDetalle(
            int clienteId,
            string nombre)
        {
            int usuarioId =
                HttpContext.Session.GetInt32("UsuarioId") ?? 0;

            string rol =
                HttpContext.Session.GetString("Rol") ?? "";

            using var conn = _context.CreateConnection();

            var ventas = rol == "ADMIN"
                ? conn.Query<Venta>(@"
                    SELECT
                        v.*,
                        p.Nombre AS ProductoNombre
                    FROM Ventas v
                    LEFT JOIN Productos p
                        ON p.Id = v.ProductoId
                    WHERE v.ClienteId = @Id
                    AND v.EsFiado = 1
                ",
                new
                {
                    Id = clienteId
                }).ToList()

                : conn.Query<Venta>(@"
                    SELECT
                        v.*,
                        p.Nombre AS ProductoNombre
                    FROM Ventas v
                    LEFT JOIN Productos p
                        ON p.Id = v.ProductoId
                    WHERE v.ClienteId = @Id
                    AND v.EsFiado = 1
                    AND v.UsuarioId = @UsuarioId
                ",
                new
                {
                    Id = clienteId,
                    UsuarioId = usuarioId
                }).ToList();

            var mensaje = $"---- {HttpContext.Session.GetString("Negocio")} ----%0A";
            mensaje += "--------------------%0A";
            mensaje += $"Cuenta: {HttpContext.Session.GetString("NumeroCuenta")}%0A%0A%0A";
            mensaje += $"Hola, {nombre}%0A";
            mensaje += "Te comparto el detalle de tu deuda :):%0A%0A";

            decimal total = 0;

            foreach (var v in ventas)
            {
                var pagado = v.Pagado ?? 0;

                var pendiente = v.Total - pagado;

                if (pendiente <= 0)
                {
                    continue;
                }

                mensaje +=
                    $"{v.ProductoNombre}: ${pendiente}%0A";

                total += pendiente;
            }

            mensaje += $"%0ATotal: ${total}%0A";
            mensaje += "Gracias por tu preferencia";

            return mensaje;
        }
    }
}