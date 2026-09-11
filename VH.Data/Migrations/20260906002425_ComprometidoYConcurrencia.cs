using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VH.Data.Migrations
{
    /// <summary>
    /// Añade la existencia comprometida y el control de concurrencia, y resuelve
    /// los renglones ya autorizados contra el stock actual.
    ///
    /// Sin ese reparto inicial, los renglones que venían autorizados quedarían en
    /// un estado que ya no existe en el flujo: ni reservados ni esperando compra,
    /// y el comprometido arrancaría en cero mientras hay material realmente
    /// prometido a alguien.
    /// </summary>
    public partial class ComprometidoYConcurrencia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Comprometido",
                table: "Inventarios",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Inventarios",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "ComprasEPPDetalle",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            // Reparto inicial de lo ya autorizado, con el mismo criterio que usa el
            // sistema a partir de ahora: por cada material y almacén, se van
            // sirviendo los renglones del más antiguo al más nuevo hasta agotar la
            // existencia. Los que caben quedan reservados (5); el resto, por
            // comprar (6).
            migrationBuilder.Sql(@"
                WITH renglones AS (
                    SELECT
                        d.[IdRequisicionDetalle],
                        d.[IdMaterial],
                        r.[IdAlmacen],
                        SUM(d.[CantidadSolicitada]) OVER (
                            PARTITION BY d.[IdMaterial], r.[IdAlmacen]
                            ORDER BY d.[IdRequisicionDetalle]
                            ROWS UNBOUNDED PRECEDING
                        ) AS [Acumulado]
                    FROM [RequisicionesEPPDetalle] d
                    INNER JOIN [RequisicionesEPP] r ON r.[IdRequisicion] = d.[IdRequisicion]
                    WHERE d.[EstadoRenglon] = 1   -- Autorizado
                )
                UPDATE d
                SET d.[EstadoRenglon] =
                    CASE WHEN x.[Acumulado] <= ISNULL(i.[Existencia], 0) THEN 5 ELSE 6 END
                FROM [RequisicionesEPPDetalle] d
                INNER JOIN renglones x ON x.[IdRequisicionDetalle] = d.[IdRequisicionDetalle]
                LEFT JOIN [Inventarios] i
                    ON i.[IdMaterial] = x.[IdMaterial] AND i.[IdAlmacen] = x.[IdAlmacen];
            ");

            // El comprometido de cada inventario es la suma de lo que quedó reservado.
            migrationBuilder.Sql(@"
                UPDATE i
                SET i.[Comprometido] = x.[Total]
                FROM [Inventarios] i
                INNER JOIN (
                    SELECT d.[IdMaterial], r.[IdAlmacen], SUM(d.[CantidadSolicitada]) AS [Total]
                    FROM [RequisicionesEPPDetalle] d
                    INNER JOIN [RequisicionesEPP] r ON r.[IdRequisicion] = d.[IdRequisicion]
                    WHERE d.[EstadoRenglon] = 5   -- Reservado
                    GROUP BY d.[IdMaterial], r.[IdAlmacen]
                ) x ON x.[IdMaterial] = i.[IdMaterial] AND x.[IdAlmacen] = i.[IdAlmacen];
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reservado y por comprar vuelven a ser simplemente autorizado.
            migrationBuilder.Sql(
                "UPDATE [RequisicionesEPPDetalle] SET [EstadoRenglon] = 1 WHERE [EstadoRenglon] IN (5, 6);");

            migrationBuilder.DropColumn(name: "Comprometido", table: "Inventarios");
            migrationBuilder.DropColumn(name: "RowVersion", table: "Inventarios");
            migrationBuilder.DropColumn(name: "RowVersion", table: "ComprasEPPDetalle");
        }
    }
}
