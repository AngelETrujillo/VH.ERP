using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VH.Data.Migrations
{
    /// <summary>
    /// Parte la compra en documento y renglones: ComprasEPP pasa a ser la factura
    /// del proveedor y cada ComprasEPPDetalle es un renglón, que es también el lote
    /// de inventario del que salen las entregas.
    ///
    /// EF generó esta migración soltando las columnas de ComprasEPP y creando la
    /// tabla de renglones vacía, lo que habría borrado todos los lotes y dejado las
    /// entregas apuntando a filas inexistentes. Se reescribió a mano para copiar los
    /// datos antes de soltar nada.
    ///
    /// La copia conserva el identificador: el renglón que nace de la compra 41 recibe
    /// IdCompraDetalle = 41. Así el simple cambio de nombre de la columna en
    /// EntregasEPP y RequisicionesEPPDetalle deja las referencias correctas, sin
    /// necesidad de una tabla de equivalencias.
    /// </summary>
    public partial class CompraCabeceraDetalle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── 1. Tabla de renglones ────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "ComprasEPPDetalle",
                columns: table => new
                {
                    IdCompraDetalle = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdCompra = table.Column<int>(type: "int", nullable: false),
                    IdMaterial = table.Column<int>(type: "int", nullable: false),
                    IdAlmacen = table.Column<int>(type: "int", nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CantidadDisponible = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Talla = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    FechaCaducidad = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComprasEPPDetalle", x => x.IdCompraDetalle);
                    table.ForeignKey(
                        name: "FK_ComprasEPPDetalle_Almacenes_IdAlmacen",
                        column: x => x.IdAlmacen,
                        principalTable: "Almacenes",
                        principalColumn: "IdAlmacen",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ComprasEPPDetalle_ComprasEPP_IdCompra",
                        column: x => x.IdCompra,
                        principalTable: "ComprasEPP",
                        principalColumn: "IdCompra",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ComprasEPPDetalle_Materiales_IdMaterial",
                        column: x => x.IdMaterial,
                        principalTable: "Materiales",
                        principalColumn: "IdMaterial",
                        onDelete: ReferentialAction.Restrict);
                });

            // ── 2. Cada compra existente se convierte en un renglón, con el mismo id ──
            migrationBuilder.Sql(@"
                SET IDENTITY_INSERT [ComprasEPPDetalle] ON;

                INSERT INTO [ComprasEPPDetalle]
                    ([IdCompraDetalle], [IdCompra], [IdMaterial], [IdAlmacen],
                     [Cantidad], [CantidadDisponible], [PrecioUnitario], [Talla], [FechaCaducidad])
                SELECT
                    [IdCompra], [IdCompra], [IdMaterial], [IdAlmacen],
                    [CantidadComprada], [CantidadDisponible], [PrecioUnitario], NULL, NULL
                FROM [ComprasEPP];

                SET IDENTITY_INSERT [ComprasEPPDetalle] OFF;
            ");

            // El identity debe continuar después del último id copiado.
            migrationBuilder.Sql(@"
                DECLARE @maxId INT = (SELECT ISNULL(MAX([IdCompraDetalle]), 0) FROM [ComprasEPPDetalle]);
                IF @maxId > 0
                    DBCC CHECKIDENT ('ComprasEPPDetalle', RESEED, @maxId);
            ");

            // ── 3. Las referencias al lote pasan a apuntar al renglón ────────
            // Los valores ya son correctos porque el renglón conservó el id.
            migrationBuilder.DropForeignKey(
                name: "FK_EntregasEPP_ComprasEPP_IdCompra",
                table: "EntregasEPP");

            migrationBuilder.DropForeignKey(
                name: "FK_RequisicionesEPPDetalle_ComprasEPP_IdCompra",
                table: "RequisicionesEPPDetalle");

            migrationBuilder.RenameColumn(
                name: "IdCompra",
                table: "EntregasEPP",
                newName: "IdCompraDetalle");

            migrationBuilder.RenameIndex(
                name: "IX_EntregasEPP_IdCompra",
                table: "EntregasEPP",
                newName: "IX_EntregasEPP_IdCompraDetalle");

            migrationBuilder.RenameColumn(
                name: "IdCompra",
                table: "RequisicionesEPPDetalle",
                newName: "IdCompraDetalle");

            migrationBuilder.RenameIndex(
                name: "IX_RequisicionesEPPDetalle_IdCompra",
                table: "RequisicionesEPPDetalle",
                newName: "IX_RequisicionesEPPDetalle_IdCompraDetalle");

            // ── 4. La cabecera se queda sólo con los datos del documento ─────
            migrationBuilder.DropForeignKey(
                name: "FK_ComprasEPP_Almacenes_IdAlmacen",
                table: "ComprasEPP");

            migrationBuilder.DropForeignKey(
                name: "FK_ComprasEPP_Materiales_IdMaterial",
                table: "ComprasEPP");

            migrationBuilder.DropIndex(
                name: "IX_ComprasEPP_IdAlmacen",
                table: "ComprasEPP");

            migrationBuilder.DropIndex(
                name: "IX_ComprasEPP_IdMaterial",
                table: "ComprasEPP");

            // PrecioUnitario se recicla como Total para no perder la columna;
            // su valor se recalcula enseguida desde los renglones.
            migrationBuilder.RenameColumn(
                name: "PrecioUnitario",
                table: "ComprasEPP",
                newName: "Total");

            migrationBuilder.AddColumn<decimal>(
                name: "Subtotal",
                table: "ComprasEPP",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Iva",
                table: "ComprasEPP",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Moneda",
                table: "ComprasEPP",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "MXN");

            migrationBuilder.AddColumn<string>(
                name: "UuidCFDI",
                table: "ComprasEPP",
                type: "nvarchar(36)",
                maxLength: 36,
                nullable: true);

            // Los importes del documento salen de sus renglones. El IVA histórico
            // no se conoce, así que queda en cero y el total iguala al subtotal.
            migrationBuilder.Sql(@"
                UPDATE c
                SET c.[Subtotal] = x.[Importe],
                    c.[Total]    = x.[Importe],
                    c.[Iva]      = 0
                FROM [ComprasEPP] c
                INNER JOIN (
                    SELECT [IdCompra], SUM([Cantidad] * [PrecioUnitario]) AS [Importe]
                    FROM [ComprasEPPDetalle]
                    GROUP BY [IdCompra]
                ) x ON x.[IdCompra] = c.[IdCompra];
            ");

            // Ya copiadas, las columnas de renglón sobran en la cabecera.
            migrationBuilder.DropColumn(name: "CantidadComprada", table: "ComprasEPP");
            migrationBuilder.DropColumn(name: "CantidadDisponible", table: "ComprasEPP");
            migrationBuilder.DropColumn(name: "IdAlmacen", table: "ComprasEPP");
            migrationBuilder.DropColumn(name: "IdMaterial", table: "ComprasEPP");

            // ── 5. Índices y claves foráneas nuevas ──────────────────────────
            migrationBuilder.CreateIndex(
                name: "IX_ComprasEPP_NumeroDocumento",
                table: "ComprasEPP",
                column: "NumeroDocumento");

            migrationBuilder.CreateIndex(
                name: "IX_ComprasEPPDetalle_FechaCaducidad",
                table: "ComprasEPPDetalle",
                column: "FechaCaducidad");

            migrationBuilder.CreateIndex(
                name: "IX_ComprasEPPDetalle_IdAlmacen",
                table: "ComprasEPPDetalle",
                column: "IdAlmacen");

            migrationBuilder.CreateIndex(
                name: "IX_ComprasEPPDetalle_IdCompra",
                table: "ComprasEPPDetalle",
                column: "IdCompra");

            migrationBuilder.CreateIndex(
                name: "IX_ComprasEPPDetalle_IdMaterial_IdAlmacen",
                table: "ComprasEPPDetalle",
                columns: new[] { "IdMaterial", "IdAlmacen" });

            migrationBuilder.AddForeignKey(
                name: "FK_EntregasEPP_ComprasEPPDetalle_IdCompraDetalle",
                table: "EntregasEPP",
                column: "IdCompraDetalle",
                principalTable: "ComprasEPPDetalle",
                principalColumn: "IdCompraDetalle",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RequisicionesEPPDetalle_ComprasEPPDetalle_IdCompraDetalle",
                table: "RequisicionesEPPDetalle",
                column: "IdCompraDetalle",
                principalTable: "ComprasEPPDetalle",
                principalColumn: "IdCompraDetalle",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EntregasEPP_ComprasEPPDetalle_IdCompraDetalle",
                table: "EntregasEPP");

            migrationBuilder.DropForeignKey(
                name: "FK_RequisicionesEPPDetalle_ComprasEPPDetalle_IdCompraDetalle",
                table: "RequisicionesEPPDetalle");

            migrationBuilder.DropIndex(
                name: "IX_ComprasEPP_NumeroDocumento",
                table: "ComprasEPP");

            // Devolver las columnas de renglón a la cabecera.
            migrationBuilder.AddColumn<decimal>(
                name: "CantidadComprada",
                table: "ComprasEPP",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CantidadDisponible",
                table: "ComprasEPP",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "IdAlmacen",
                table: "ComprasEPP",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "IdMaterial",
                table: "ComprasEPP",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Sólo se puede recuperar un renglón por compra: el primero.
            migrationBuilder.Sql(@"
                UPDATE c
                SET c.[IdMaterial]         = d.[IdMaterial],
                    c.[IdAlmacen]          = d.[IdAlmacen],
                    c.[CantidadComprada]   = d.[Cantidad],
                    c.[CantidadDisponible] = d.[CantidadDisponible],
                    c.[Total]              = d.[PrecioUnitario]
                FROM [ComprasEPP] c
                INNER JOIN (
                    SELECT [IdCompra], [IdMaterial], [IdAlmacen], [Cantidad],
                           [CantidadDisponible], [PrecioUnitario],
                           ROW_NUMBER() OVER (PARTITION BY [IdCompra] ORDER BY [IdCompraDetalle]) AS rn
                    FROM [ComprasEPPDetalle]
                ) d ON d.[IdCompra] = c.[IdCompra] AND d.rn = 1;
            ");

            migrationBuilder.DropTable(name: "ComprasEPPDetalle");

            migrationBuilder.DropColumn(name: "Iva", table: "ComprasEPP");
            migrationBuilder.DropColumn(name: "Moneda", table: "ComprasEPP");
            migrationBuilder.DropColumn(name: "Subtotal", table: "ComprasEPP");
            migrationBuilder.DropColumn(name: "UuidCFDI", table: "ComprasEPP");

            migrationBuilder.RenameColumn(
                name: "Total",
                table: "ComprasEPP",
                newName: "PrecioUnitario");

            migrationBuilder.RenameColumn(
                name: "IdCompraDetalle",
                table: "RequisicionesEPPDetalle",
                newName: "IdCompra");

            migrationBuilder.RenameIndex(
                name: "IX_RequisicionesEPPDetalle_IdCompraDetalle",
                table: "RequisicionesEPPDetalle",
                newName: "IX_RequisicionesEPPDetalle_IdCompra");

            migrationBuilder.RenameColumn(
                name: "IdCompraDetalle",
                table: "EntregasEPP",
                newName: "IdCompra");

            migrationBuilder.RenameIndex(
                name: "IX_EntregasEPP_IdCompraDetalle",
                table: "EntregasEPP",
                newName: "IX_EntregasEPP_IdCompra");

            migrationBuilder.CreateIndex(
                name: "IX_ComprasEPP_IdAlmacen",
                table: "ComprasEPP",
                column: "IdAlmacen");

            migrationBuilder.CreateIndex(
                name: "IX_ComprasEPP_IdMaterial",
                table: "ComprasEPP",
                column: "IdMaterial");

            migrationBuilder.AddForeignKey(
                name: "FK_ComprasEPP_Almacenes_IdAlmacen",
                table: "ComprasEPP",
                column: "IdAlmacen",
                principalTable: "Almacenes",
                principalColumn: "IdAlmacen",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ComprasEPP_Materiales_IdMaterial",
                table: "ComprasEPP",
                column: "IdMaterial",
                principalTable: "Materiales",
                principalColumn: "IdMaterial",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EntregasEPP_ComprasEPP_IdCompra",
                table: "EntregasEPP",
                column: "IdCompra",
                principalTable: "ComprasEPP",
                principalColumn: "IdCompra",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RequisicionesEPPDetalle_ComprasEPP_IdCompra",
                table: "RequisicionesEPPDetalle",
                column: "IdCompra",
                principalTable: "ComprasEPP",
                principalColumn: "IdCompra",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
