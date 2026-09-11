using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VH.Data.Migrations
{
    /// <inheritdoc />
    public partial class KardexMovimientosInventario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MovimientosInventario",
                columns: table => new
                {
                    IdMovimiento = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdMaterial = table.Column<int>(type: "int", nullable: false),
                    IdAlmacen = table.Column<int>(type: "int", nullable: false),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CostoUnitario = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    IdCompraDetalle = table.Column<int>(type: "int", nullable: true),
                    DocumentoTipo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DocumentoId = table.Column<int>(type: "int", nullable: true),
                    DocumentoFolio = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IdUsuario = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    SaldoResultante = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimientosInventario", x => x.IdMovimiento);
                    table.ForeignKey(
                        name: "FK_MovimientosInventario_Almacenes_IdAlmacen",
                        column: x => x.IdAlmacen,
                        principalTable: "Almacenes",
                        principalColumn: "IdAlmacen",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimientosInventario_AspNetUsers_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimientosInventario_ComprasEPPDetalle_IdCompraDetalle",
                        column: x => x.IdCompraDetalle,
                        principalTable: "ComprasEPPDetalle",
                        principalColumn: "IdCompraDetalle",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimientosInventario_Materiales_IdMaterial",
                        column: x => x.IdMaterial,
                        principalTable: "Materiales",
                        principalColumn: "IdMaterial",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_DocumentoTipo_DocumentoId",
                table: "MovimientosInventario",
                columns: new[] { "DocumentoTipo", "DocumentoId" });

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_Fecha",
                table: "MovimientosInventario",
                column: "Fecha");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_IdAlmacen",
                table: "MovimientosInventario",
                column: "IdAlmacen");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_IdCompraDetalle",
                table: "MovimientosInventario",
                column: "IdCompraDetalle");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_IdMaterial_IdAlmacen_Fecha",
                table: "MovimientosInventario",
                columns: new[] { "IdMaterial", "IdAlmacen", "Fecha" });

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_IdUsuario",
                table: "MovimientosInventario",
                column: "IdUsuario");

            // ===== RECONSTRUCCIÓN DEL HISTORIAL =====
            //
            // El kardex nace con la historia que ya se puede demostrar: cada lote
            // comprado es una entrada y cada entrega firmada es una salida. Lo que
            // sobre o falte contra la existencia registrada se asienta como un
            // ajuste de saldo inicial, para que desde el primer día no quede
            // existencia sin un movimiento que la explique.

            // 1. Entradas: un renglón por cada lote comprado.
            migrationBuilder.Sql(@"
                INSERT INTO MovimientosInventario
                    (IdMaterial, IdAlmacen, Tipo, Cantidad, CostoUnitario, IdCompraDetalle,
                     DocumentoTipo, DocumentoId, DocumentoFolio, Observaciones, Fecha, IdUsuario, SaldoResultante)
                SELECT
                    d.IdMaterial, d.IdAlmacen, 0, d.Cantidad, d.PrecioUnitario, d.IdCompraDetalle,
                    'CompraEPP', c.IdCompra, NULLIF(LEFT(ISNULL(c.NumeroDocumento, ''), 50), ''),
                    'Reconstruido del historial de compras al iniciar el kardex.',
                    c.FechaCompra, NULL, 0
                FROM ComprasEPPDetalle d
                INNER JOIN ComprasEPP c ON c.IdCompra = d.IdCompra;");

            // 2. Salidas: un renglón por cada entrega a un trabajador.
            migrationBuilder.Sql(@"
                INSERT INTO MovimientosInventario
                    (IdMaterial, IdAlmacen, Tipo, Cantidad, CostoUnitario, IdCompraDetalle,
                     DocumentoTipo, DocumentoId, DocumentoFolio, Observaciones, Fecha, IdUsuario, SaldoResultante)
                SELECT
                    d.IdMaterial, d.IdAlmacen, 1, -e.CantidadEntregada, d.PrecioUnitario, e.IdCompraDetalle,
                    'EntregaEPP', e.IdEntrega, NULL,
                    'Reconstruido del historial de entregas al iniciar el kardex.',
                    e.FechaEntrega, NULL, 0
                FROM EntregasEPP e
                INNER JOIN ComprasEPPDetalle d ON d.IdCompraDetalle = e.IdCompraDetalle;");

            // 3. Ajuste de apertura por la existencia que la historia no explica.
            migrationBuilder.Sql(@"
                INSERT INTO MovimientosInventario
                    (IdMaterial, IdAlmacen, Tipo, Cantidad, CostoUnitario, IdCompraDetalle,
                     DocumentoTipo, DocumentoId, DocumentoFolio, Observaciones, Fecha, IdUsuario, SaldoResultante)
                SELECT
                    i.IdMaterial, i.IdAlmacen, 4, i.Existencia - ISNULL(h.Suma, 0), NULL, NULL,
                    'Ajuste', NULL, NULL,
                    'Saldo inicial: existencia registrada antes del kardex, sin documento que la respalde.',
                    SYSDATETIME(), NULL, 0
                FROM Inventarios i
                LEFT JOIN (
                    SELECT IdMaterial, IdAlmacen, SUM(Cantidad) AS Suma
                    FROM MovimientosInventario
                    GROUP BY IdMaterial, IdAlmacen
                ) h ON h.IdMaterial = i.IdMaterial AND h.IdAlmacen = i.IdAlmacen
                WHERE i.Existencia - ISNULL(h.Suma, 0) <> 0;");

            // 4. Apartados vigentes, para que el comprometido también tenga origen.
            migrationBuilder.Sql(@"
                INSERT INTO MovimientosInventario
                    (IdMaterial, IdAlmacen, Tipo, Cantidad, CostoUnitario, IdCompraDetalle,
                     DocumentoTipo, DocumentoId, DocumentoFolio, Observaciones, Fecha, IdUsuario, SaldoResultante)
                SELECT
                    i.IdMaterial, i.IdAlmacen, 2, i.Comprometido, NULL, NULL,
                    'RequisicionEPP', NULL, NULL,
                    'Apartado vigente al iniciar el kardex.',
                    SYSDATETIME(), NULL, i.Existencia
                FROM Inventarios i
                WHERE i.Comprometido > 0;");

            // 5. Saldo corrido: cada renglón queda con la existencia que dejó tras de sí.
            //    Las reservas no entran: apartan, pero no mueven el anaquel.
            migrationBuilder.Sql(@"
                WITH Corrido AS (
                    SELECT
                        IdMovimiento,
                        SUM(Cantidad) OVER (
                            PARTITION BY IdMaterial, IdAlmacen
                            ORDER BY Fecha, IdMovimiento
                            ROWS UNBOUNDED PRECEDING) AS Saldo
                    FROM MovimientosInventario
                    WHERE Tipo NOT IN (2, 3)
                )
                UPDATE m
                SET m.SaldoResultante = c.Saldo
                FROM MovimientosInventario m
                INNER JOIN Corrido c ON c.IdMovimiento = m.IdMovimiento;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MovimientosInventario");
        }
    }
}
