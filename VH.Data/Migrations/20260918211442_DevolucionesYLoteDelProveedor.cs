using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VH.Data.Migrations
{
    /// <inheritdoc />
    public partial class DevolucionesYLoteDelProveedor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LoteProveedor",
                table: "ComprasEPPDetalle",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            // El lote de fábrica se venía capturando en la recepción, pero no
            // viajaba al lote de inventario. Se copia hacia atrás para que lo ya
            // recibido también se pueda rastrear: si no, el dato sólo serviría
            // para lo que entre de hoy en adelante.
            migrationBuilder.Sql(@"
                UPDATE cd
                SET cd.LoteProveedor = rd.LoteProveedor
                FROM ComprasEPPDetalle cd
                INNER JOIN RecepcionesCompraDetalle rd
                    ON rd.IdCompraDetalle = cd.IdCompraDetalle
                WHERE rd.LoteProveedor IS NOT NULL
                  AND cd.LoteProveedor IS NULL;");

            migrationBuilder.CreateTable(
                name: "DevolucionesEPP",
                columns: table => new
                {
                    IdDevolucion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdEntrega = table.Column<int>(type: "int", nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    FechaDevolucion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IdUsuarioRecibe = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DevolucionesEPP", x => x.IdDevolucion);
                    table.ForeignKey(
                        name: "FK_DevolucionesEPP_AspNetUsers_IdUsuarioRecibe",
                        column: x => x.IdUsuarioRecibe,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DevolucionesEPP_EntregasEPP_IdEntrega",
                        column: x => x.IdEntrega,
                        principalTable: "EntregasEPP",
                        principalColumn: "IdEntrega",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ComprasEPPDetalle_LoteProveedor",
                table: "ComprasEPPDetalle",
                column: "LoteProveedor");

            migrationBuilder.CreateIndex(
                name: "IX_DevolucionesEPP_FechaDevolucion",
                table: "DevolucionesEPP",
                column: "FechaDevolucion");

            migrationBuilder.CreateIndex(
                name: "IX_DevolucionesEPP_IdEntrega",
                table: "DevolucionesEPP",
                column: "IdEntrega");

            migrationBuilder.CreateIndex(
                name: "IX_DevolucionesEPP_IdUsuarioRecibe",
                table: "DevolucionesEPP",
                column: "IdUsuarioRecibe");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DevolucionesEPP");

            migrationBuilder.DropIndex(
                name: "IX_ComprasEPPDetalle_LoteProveedor",
                table: "ComprasEPPDetalle");

            migrationBuilder.DropColumn(
                name: "LoteProveedor",
                table: "ComprasEPPDetalle");
        }
    }
}
