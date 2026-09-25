using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VH.Data.Migrations
{
    /// <inheritdoc />
    public partial class RecepcionDeMaterial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RecepcionesCompra",
                columns: table => new
                {
                    IdRecepcion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Folio = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IdOrdenCompra = table.Column<int>(type: "int", nullable: false),
                    IdAlmacen = table.Column<int>(type: "int", nullable: false),
                    FechaRecepcion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IdUsuarioRecibe = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    NumeroFactura = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UuidCFDI = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IdCompra = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecepcionesCompra", x => x.IdRecepcion);
                    table.ForeignKey(
                        name: "FK_RecepcionesCompra_Almacenes_IdAlmacen",
                        column: x => x.IdAlmacen,
                        principalTable: "Almacenes",
                        principalColumn: "IdAlmacen",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecepcionesCompra_AspNetUsers_IdUsuarioRecibe",
                        column: x => x.IdUsuarioRecibe,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecepcionesCompra_ComprasEPP_IdCompra",
                        column: x => x.IdCompra,
                        principalTable: "ComprasEPP",
                        principalColumn: "IdCompra",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_RecepcionesCompra_OrdenesCompra_IdOrdenCompra",
                        column: x => x.IdOrdenCompra,
                        principalTable: "OrdenesCompra",
                        principalColumn: "IdOrdenCompra",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RecepcionesCompraDetalle",
                columns: table => new
                {
                    IdRecepcionDetalle = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdRecepcion = table.Column<int>(type: "int", nullable: false),
                    IdOrdenCompraDetalle = table.Column<int>(type: "int", nullable: false),
                    IdMaterial = table.Column<int>(type: "int", nullable: false),
                    CantidadRecibida = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CantidadAceptada = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PrecioUnitarioReal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Talla = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    FechaCaducidad = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LoteProveedor = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MotivoRechazo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IdCompraDetalle = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecepcionesCompraDetalle", x => x.IdRecepcionDetalle);
                    table.ForeignKey(
                        name: "FK_RecepcionesCompraDetalle_ComprasEPPDetalle_IdCompraDetalle",
                        column: x => x.IdCompraDetalle,
                        principalTable: "ComprasEPPDetalle",
                        principalColumn: "IdCompraDetalle",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_RecepcionesCompraDetalle_Materiales_IdMaterial",
                        column: x => x.IdMaterial,
                        principalTable: "Materiales",
                        principalColumn: "IdMaterial",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecepcionesCompraDetalle_OrdenesCompraDetalle_IdOrdenCompraDetalle",
                        column: x => x.IdOrdenCompraDetalle,
                        principalTable: "OrdenesCompraDetalle",
                        principalColumn: "IdOrdenCompraDetalle",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecepcionesCompraDetalle_RecepcionesCompra_IdRecepcion",
                        column: x => x.IdRecepcion,
                        principalTable: "RecepcionesCompra",
                        principalColumn: "IdRecepcion",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RecepcionesCompra_FechaRecepcion",
                table: "RecepcionesCompra",
                column: "FechaRecepcion");

            migrationBuilder.CreateIndex(
                name: "IX_RecepcionesCompra_Folio",
                table: "RecepcionesCompra",
                column: "Folio",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecepcionesCompra_IdAlmacen",
                table: "RecepcionesCompra",
                column: "IdAlmacen");

            migrationBuilder.CreateIndex(
                name: "IX_RecepcionesCompra_IdCompra",
                table: "RecepcionesCompra",
                column: "IdCompra");

            migrationBuilder.CreateIndex(
                name: "IX_RecepcionesCompra_IdOrdenCompra_IdAlmacen",
                table: "RecepcionesCompra",
                columns: new[] { "IdOrdenCompra", "IdAlmacen" });

            migrationBuilder.CreateIndex(
                name: "IX_RecepcionesCompra_IdUsuarioRecibe",
                table: "RecepcionesCompra",
                column: "IdUsuarioRecibe");

            migrationBuilder.CreateIndex(
                name: "IX_RecepcionesCompraDetalle_IdCompraDetalle",
                table: "RecepcionesCompraDetalle",
                column: "IdCompraDetalle");

            migrationBuilder.CreateIndex(
                name: "IX_RecepcionesCompraDetalle_IdMaterial",
                table: "RecepcionesCompraDetalle",
                column: "IdMaterial");

            migrationBuilder.CreateIndex(
                name: "IX_RecepcionesCompraDetalle_IdOrdenCompraDetalle",
                table: "RecepcionesCompraDetalle",
                column: "IdOrdenCompraDetalle");

            migrationBuilder.CreateIndex(
                name: "IX_RecepcionesCompraDetalle_IdRecepcion",
                table: "RecepcionesCompraDetalle",
                column: "IdRecepcion");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RecepcionesCompraDetalle");

            migrationBuilder.DropTable(
                name: "RecepcionesCompra");
        }
    }
}
