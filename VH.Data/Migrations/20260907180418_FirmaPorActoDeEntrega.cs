using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VH.Data.Migrations
{
    /// <inheritdoc />
    public partial class FirmaPorActoDeEntrega : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RequisicionesEntregas_IdRequisicion_IdEmpleado",
                table: "RequisicionesEntregas");

            migrationBuilder.AddColumn<int>(
                name: "IdRequisicionEntrega",
                table: "RequisicionesEPPDetalle",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RequisicionesEPPDetalle_IdRequisicionEntrega",
                table: "RequisicionesEPPDetalle",
                column: "IdRequisicionEntrega");

            migrationBuilder.CreateIndex(
                name: "IX_RequisicionesEntregas_IdRequisicion_IdEmpleado",
                table: "RequisicionesEntregas",
                columns: new[] { "IdRequisicion", "IdEmpleado" });

            migrationBuilder.AddForeignKey(
                name: "FK_RequisicionesEPPDetalle_RequisicionesEntregas_IdRequisicionEntrega",
                table: "RequisicionesEPPDetalle",
                column: "IdRequisicionEntrega",
                principalTable: "RequisicionesEntregas",
                principalColumn: "IdRequisicionEntrega",
                onDelete: ReferentialAction.Restrict);

            // Los renglones ya surtidos también tienen su firma: hasta hoy sólo
            // podía haber una por persona y documento, así que la correspondencia
            // es única y se puede reconstruir sin ambigüedad. Sin esto, todo lo
            // entregado antes de este cambio quedaría sin respaldo visible.
            migrationBuilder.Sql(@"
                UPDATE d
                SET d.IdRequisicionEntrega = e.IdRequisicionEntrega
                FROM RequisicionesEPPDetalle d
                INNER JOIN RequisicionesEntregas e
                    ON e.IdRequisicion = d.IdRequisicion
                   AND e.IdEmpleado = d.IdEmpleadoDestino
                WHERE d.EstadoRenglon = 3
                  AND d.IdEmpleadoDestino IS NOT NULL
                  AND d.IdRequisicionEntrega IS NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RequisicionesEPPDetalle_RequisicionesEntregas_IdRequisicionEntrega",
                table: "RequisicionesEPPDetalle");

            migrationBuilder.DropIndex(
                name: "IX_RequisicionesEPPDetalle_IdRequisicionEntrega",
                table: "RequisicionesEPPDetalle");

            migrationBuilder.DropIndex(
                name: "IX_RequisicionesEntregas_IdRequisicion_IdEmpleado",
                table: "RequisicionesEntregas");

            migrationBuilder.DropColumn(
                name: "IdRequisicionEntrega",
                table: "RequisicionesEPPDetalle");

            migrationBuilder.CreateIndex(
                name: "IX_RequisicionesEntregas_IdRequisicion_IdEmpleado",
                table: "RequisicionesEntregas",
                columns: new[] { "IdRequisicion", "IdEmpleado" },
                unique: true);
        }
    }
}
