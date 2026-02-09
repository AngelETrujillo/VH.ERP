using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VH.Data.Migrations
{
    /// <inheritdoc />
    public partial class SistemaInventarios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RequisicionesEPP",
                columns: table => new
                {
                    IdRequisicion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NumeroRequisicion = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IdUsuarioSolicita = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IdEmpleadoRecibe = table.Column<int>(type: "int", nullable: false),
                    IdAlmacen = table.Column<int>(type: "int", nullable: false),
                    Justificacion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    EstadoRequisicion = table.Column<int>(type: "int", nullable: false),
                    IdUsuarioAprueba = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    FechaAprobacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MotivoRechazo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IdUsuarioEntrega = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    FechaEntrega = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FirmaDigital = table.Column<string>(type: "nvarchar(max)", maxLength: 500000, nullable: true),
                    FotoEvidencia = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FechaSolicitud = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequisicionesEPP", x => x.IdRequisicion);
                    table.ForeignKey(
                        name: "FK_RequisicionesEPP_Almacenes_IdAlmacen",
                        column: x => x.IdAlmacen,
                        principalTable: "Almacenes",
                        principalColumn: "IdAlmacen",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RequisicionesEPP_AspNetUsers_IdUsuarioAprueba",
                        column: x => x.IdUsuarioAprueba,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RequisicionesEPP_AspNetUsers_IdUsuarioEntrega",
                        column: x => x.IdUsuarioEntrega,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RequisicionesEPP_AspNetUsers_IdUsuarioSolicita",
                        column: x => x.IdUsuarioSolicita,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RequisicionesEPP_Empleados_IdEmpleadoRecibe",
                        column: x => x.IdEmpleadoRecibe,
                        principalTable: "Empleados",
                        principalColumn: "IdEmpleado",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RequisicionesEPPDetalle",
                columns: table => new
                {
                    IdRequisicionDetalle = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdRequisicion = table.Column<int>(type: "int", nullable: false),
                    IdMaterial = table.Column<int>(type: "int", nullable: false),
                    CantidadSolicitada = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TallaSolicitada = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IdCompra = table.Column<int>(type: "int", nullable: true),
                    CantidadEntregada = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequisicionesEPPDetalle", x => x.IdRequisicionDetalle);
                    table.ForeignKey(
                        name: "FK_RequisicionesEPPDetalle_ComprasEPP_IdCompra",
                        column: x => x.IdCompra,
                        principalTable: "ComprasEPP",
                        principalColumn: "IdCompra",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RequisicionesEPPDetalle_MaterialesEPP_IdMaterial",
                        column: x => x.IdMaterial,
                        principalTable: "MaterialesEPP",
                        principalColumn: "IdMaterial",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RequisicionesEPPDetalle_RequisicionesEPP_IdRequisicion",
                        column: x => x.IdRequisicion,
                        principalTable: "RequisicionesEPP",
                        principalColumn: "IdRequisicion",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RequisicionesEPP_EstadoRequisicion",
                table: "RequisicionesEPP",
                column: "EstadoRequisicion");

            migrationBuilder.CreateIndex(
                name: "IX_RequisicionesEPP_FechaSolicitud",
                table: "RequisicionesEPP",
                column: "FechaSolicitud");

            migrationBuilder.CreateIndex(
                name: "IX_RequisicionesEPP_IdAlmacen",
                table: "RequisicionesEPP",
                column: "IdAlmacen");

            migrationBuilder.CreateIndex(
                name: "IX_RequisicionesEPP_IdEmpleadoRecibe",
                table: "RequisicionesEPP",
                column: "IdEmpleadoRecibe");

            migrationBuilder.CreateIndex(
                name: "IX_RequisicionesEPP_IdUsuarioAprueba",
                table: "RequisicionesEPP",
                column: "IdUsuarioAprueba");

            migrationBuilder.CreateIndex(
                name: "IX_RequisicionesEPP_IdUsuarioEntrega",
                table: "RequisicionesEPP",
                column: "IdUsuarioEntrega");

            migrationBuilder.CreateIndex(
                name: "IX_RequisicionesEPP_IdUsuarioSolicita",
                table: "RequisicionesEPP",
                column: "IdUsuarioSolicita");

            migrationBuilder.CreateIndex(
                name: "IX_RequisicionesEPP_NumeroRequisicion",
                table: "RequisicionesEPP",
                column: "NumeroRequisicion",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RequisicionesEPPDetalle_IdCompra",
                table: "RequisicionesEPPDetalle",
                column: "IdCompra");

            migrationBuilder.CreateIndex(
                name: "IX_RequisicionesEPPDetalle_IdMaterial",
                table: "RequisicionesEPPDetalle",
                column: "IdMaterial");

            migrationBuilder.CreateIndex(
                name: "IX_RequisicionesEPPDetalle_IdRequisicion_IdMaterial",
                table: "RequisicionesEPPDetalle",
                columns: new[] { "IdRequisicion", "IdMaterial" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RequisicionesEPPDetalle");

            migrationBuilder.DropTable(
                name: "RequisicionesEPP");
        }
    }
}
