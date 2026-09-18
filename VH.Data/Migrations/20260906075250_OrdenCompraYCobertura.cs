using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VH.Data.Migrations
{
    /// <inheritdoc />
    public partial class OrdenCompraYCobertura : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrdenesCompra",
                columns: table => new
                {
                    IdOrdenCompra = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Folio = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IdProveedor = table.Column<int>(type: "int", nullable: false),
                    FechaEmision = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaEntregaEstimada = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IdUsuarioEmite = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    Moneda = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MotivoCancelacion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdenesCompra", x => x.IdOrdenCompra);
                    table.ForeignKey(
                        name: "FK_OrdenesCompra_AspNetUsers_IdUsuarioEmite",
                        column: x => x.IdUsuarioEmite,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrdenesCompra_Proveedores_IdProveedor",
                        column: x => x.IdProveedor,
                        principalTable: "Proveedores",
                        principalColumn: "IdProveedor",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrdenesCompraDetalle",
                columns: table => new
                {
                    IdOrdenCompraDetalle = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdOrdenCompra = table.Column<int>(type: "int", nullable: false),
                    IdMaterial = table.Column<int>(type: "int", nullable: false),
                    IdAlmacenDestino = table.Column<int>(type: "int", nullable: false),
                    CantidadPedida = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CantidadRecibida = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PrecioUnitarioPactado = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdenesCompraDetalle", x => x.IdOrdenCompraDetalle);
                    table.ForeignKey(
                        name: "FK_OrdenesCompraDetalle_Almacenes_IdAlmacenDestino",
                        column: x => x.IdAlmacenDestino,
                        principalTable: "Almacenes",
                        principalColumn: "IdAlmacen",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrdenesCompraDetalle_Materiales_IdMaterial",
                        column: x => x.IdMaterial,
                        principalTable: "Materiales",
                        principalColumn: "IdMaterial",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrdenesCompraDetalle_OrdenesCompra_IdOrdenCompra",
                        column: x => x.IdOrdenCompra,
                        principalTable: "OrdenesCompra",
                        principalColumn: "IdOrdenCompra",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RequisicionesCobertura",
                columns: table => new
                {
                    IdRequisicionCobertura = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdRequisicionDetalle = table.Column<int>(type: "int", nullable: false),
                    Origen = table.Column<int>(type: "int", nullable: false),
                    IdOrdenCompraDetalle = table.Column<int>(type: "int", nullable: true),
                    Cantidad = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequisicionesCobertura", x => x.IdRequisicionCobertura);
                    table.ForeignKey(
                        name: "FK_RequisicionesCobertura_OrdenesCompraDetalle_IdOrdenCompraDetalle",
                        column: x => x.IdOrdenCompraDetalle,
                        principalTable: "OrdenesCompraDetalle",
                        principalColumn: "IdOrdenCompraDetalle",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RequisicionesCobertura_RequisicionesEPPDetalle_IdRequisicionDetalle",
                        column: x => x.IdRequisicionDetalle,
                        principalTable: "RequisicionesEPPDetalle",
                        principalColumn: "IdRequisicionDetalle",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesCompra_Estado",
                table: "OrdenesCompra",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesCompra_FechaEmision",
                table: "OrdenesCompra",
                column: "FechaEmision");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesCompra_Folio",
                table: "OrdenesCompra",
                column: "Folio",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesCompra_IdProveedor",
                table: "OrdenesCompra",
                column: "IdProveedor");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesCompra_IdUsuarioEmite",
                table: "OrdenesCompra",
                column: "IdUsuarioEmite");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesCompraDetalle_IdAlmacenDestino",
                table: "OrdenesCompraDetalle",
                column: "IdAlmacenDestino");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesCompraDetalle_IdMaterial_IdAlmacenDestino",
                table: "OrdenesCompraDetalle",
                columns: new[] { "IdMaterial", "IdAlmacenDestino" });

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesCompraDetalle_IdOrdenCompra",
                table: "OrdenesCompraDetalle",
                column: "IdOrdenCompra");

            migrationBuilder.CreateIndex(
                name: "IX_RequisicionesCobertura_IdOrdenCompraDetalle",
                table: "RequisicionesCobertura",
                column: "IdOrdenCompraDetalle");

            migrationBuilder.CreateIndex(
                name: "IX_RequisicionesCobertura_IdRequisicionDetalle",
                table: "RequisicionesCobertura",
                column: "IdRequisicionDetalle");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RequisicionesCobertura");

            migrationBuilder.DropTable(
                name: "OrdenesCompraDetalle");

            migrationBuilder.DropTable(
                name: "OrdenesCompra");
        }
    }
}
