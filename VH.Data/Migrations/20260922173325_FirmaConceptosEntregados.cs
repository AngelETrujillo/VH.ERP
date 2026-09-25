using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VH.Data.Migrations
{
    /// <inheritdoc />
    public partial class FirmaConceptosEntregados : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdRequisicionEntrega",
                table: "EntregasEPP",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EntregasEPP_IdRequisicionEntrega",
                table: "EntregasEPP",
                column: "IdRequisicionEntrega");

            migrationBuilder.AddForeignKey(
                name: "FK_EntregasEPP_RequisicionesEntregas_IdRequisicionEntrega",
                table: "EntregasEPP",
                column: "IdRequisicionEntrega",
                principalTable: "RequisicionesEntregas",
                principalColumn: "IdRequisicionEntrega",
                onDelete: ReferentialAction.Restrict);

            // Relleno de lo ya entregado. Las salidas que nacieron de una
            // requisición guardan su folio en Observaciones ("Requisición: REQ-...");
            // de ahí sale a qué documento pertenecen. Dentro de ese documento, la
            // firma de esa misma persona más cercana en el tiempo es la que las
            // amparó: cuando alguien firma dos veces, los actos están separados por
            // minutos y las salidas caen del lado correcto.
            //
            // Las entregas sueltas, que no vienen de ninguna requisición, se quedan
            // en NULL a propósito: no hay firma que enlazarles.
            migrationBuilder.Sql(@"
                UPDATE e
                SET e.IdRequisicionEntrega = x.IdRequisicionEntrega
                FROM EntregasEPP e
                CROSS APPLY (
                    SELECT TOP 1 f.IdRequisicionEntrega
                    FROM RequisicionesEntregas f
                    INNER JOIN RequisicionesEPP r ON r.IdRequisicion = f.IdRequisicion
                    WHERE f.IdEmpleado = e.IdEmpleado
                      AND r.NumeroRequisicion = LTRIM(RTRIM(
                            SUBSTRING(e.Observaciones,
                                      CHARINDEX(N': ', e.Observaciones) + 2,
                                      LEN(e.Observaciones))))
                    ORDER BY ABS(DATEDIFF(SECOND, f.FechaEntrega, e.FechaEntrega))
                ) x
                WHERE e.IdRequisicionEntrega IS NULL
                  AND e.Observaciones IS NOT NULL
                  AND CHARINDEX(N': ', e.Observaciones) > 0;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EntregasEPP_RequisicionesEntregas_IdRequisicionEntrega",
                table: "EntregasEPP");

            migrationBuilder.DropIndex(
                name: "IX_EntregasEPP_IdRequisicionEntrega",
                table: "EntregasEPP");

            migrationBuilder.DropColumn(
                name: "IdRequisicionEntrega",
                table: "EntregasEPP");
        }
    }
}
