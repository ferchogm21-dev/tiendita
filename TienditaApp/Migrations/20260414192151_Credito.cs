using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TienditaApp.Migrations
{
    /// <inheritdoc />
    public partial class Credito : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EsCredito",
                table: "Ventas",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "Pagado",
                table: "Ventas",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EsCredito",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "Pagado",
                table: "Ventas");
        }
    }
}
