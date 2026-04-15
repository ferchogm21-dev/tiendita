using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TienditaApp.Migrations
{
    /// <inheritdoc />
    public partial class AgregarFechaPago : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaPago",
                table: "Ventas",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FechaPago",
                table: "Ventas");
        }
    }
}
