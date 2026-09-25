using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VH.Data.Migrations
{
    /// <inheritdoc />
    public partial class ConsumoDeObraYFuenteUnica : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequiereDevolucion",
                table: "ConfiguracionesMaterialEPP");

            migrationBuilder.DropColumn(
                name: "VidaUtilDias",
                table: "ConfiguracionesMaterialEPP");

            migrationBuilder.AlterColumn<int>(
                name: "IdEmpleado",
                table: "EntregasEPP",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "IdConceptoPartida",
                table: "EntregasEPP",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdProyectoDestino",
                table: "EntregasEPP",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdRequisicionDetalle",
                table: "EntregasEPP",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EntregasEPP_IdConceptoPartida",
                table: "EntregasEPP",
                column: "IdConceptoPartida");

            migrationBuilder.CreateIndex(
                name: "IX_EntregasEPP_IdProyectoDestino",
                table: "EntregasEPP",
                column: "IdProyectoDestino");

            migrationBuilder.CreateIndex(
                name: "IX_EntregasEPP_IdRequisicionDetalle",
                table: "EntregasEPP",
                column: "IdRequisicionDetalle");

            migrationBuilder.AddForeignKey(
                name: "FK_EntregasEPP_ConceptosPartidas_IdConceptoPartida",
                table: "EntregasEPP",
                column: "IdConceptoPartida",
                principalTable: "ConceptosPartidas",
                principalColumn: "IdPartida");

            migrationBuilder.AddForeignKey(
                name: "FK_EntregasEPP_Proyectos_IdProyectoDestino",
                table: "EntregasEPP",
                column: "IdProyectoDestino",
                principalTable: "Proyectos",
                principalColumn: "IdProyecto");

            migrationBuilder.AddForeignKey(
                name: "FK_EntregasEPP_RequisicionesEPPDetalle_IdRequisicionDetalle",
                table: "EntregasEPP",
                column: "IdRequisicionDetalle",
                principalTable: "RequisicionesEPPDetalle",
                principalColumn: "IdRequisicionDetalle");

            // Relleno del enlace al renglón para las salidas que ya existen.
            //
            // Sólo se ata lo inequívoco: desde la firma se llega a la requisición, y
            // dentro de ella se busca el renglón del mismo material para la misma
            // persona. Cuando hay más de un candidato -y las hay: seis requisiciones
            // repiten material- se deja en NULL en vez de adivinar. Un enlace
            // inventado es peor que ninguno, porque la revisión de integridad lo
            // daría por bueno.
            migrationBuilder.Sql(@"
                UPDATE e
                SET e.IdRequisicionDetalle = x.IdRequisicionDetalle
                FROM EntregasEPP e
                INNER JOIN RequisicionesEntregas f ON f.IdRequisicionEntrega = e.IdRequisicionEntrega
                INNER JOIN ComprasEPPDetalle cd ON cd.IdCompraDetalle = e.IdCompraDetalle
                CROSS APPLY (
                    SELECT TOP 2 d.IdRequisicionDetalle
                    FROM RequisicionesEPPDetalle d
                    WHERE d.IdRequisicion = f.IdRequisicion
                      AND d.IdMaterial = cd.IdMaterial
                      AND d.IdEmpleadoDestino = e.IdEmpleado
                ) x
                WHERE e.IdRequisicionDetalle IS NULL
                  AND 1 = (
                    SELECT COUNT(*)
                    FROM RequisicionesEPPDetalle d2
                    WHERE d2.IdRequisicion = f.IdRequisicion
                      AND d2.IdMaterial = cd.IdMaterial
                      AND d2.IdEmpleadoDestino = e.IdEmpleado
                  );
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EntregasEPP_ConceptosPartidas_IdConceptoPartida",
                table: "EntregasEPP");

            migrationBuilder.DropForeignKey(
                name: "FK_EntregasEPP_Proyectos_IdProyectoDestino",
                table: "EntregasEPP");

            migrationBuilder.DropForeignKey(
                name: "FK_EntregasEPP_RequisicionesEPPDetalle_IdRequisicionDetalle",
                table: "EntregasEPP");

            migrationBuilder.DropIndex(
                name: "IX_EntregasEPP_IdConceptoPartida",
                table: "EntregasEPP");

            migrationBuilder.DropIndex(
                name: "IX_EntregasEPP_IdProyectoDestino",
                table: "EntregasEPP");

            migrationBuilder.DropIndex(
                name: "IX_EntregasEPP_IdRequisicionDetalle",
                table: "EntregasEPP");

            migrationBuilder.DropColumn(
                name: "IdConceptoPartida",
                table: "EntregasEPP");

            migrationBuilder.DropColumn(
                name: "IdProyectoDestino",
                table: "EntregasEPP");

            migrationBuilder.DropColumn(
                name: "IdRequisicionDetalle",
                table: "EntregasEPP");

            migrationBuilder.AlterColumn<int>(
                name: "IdEmpleado",
                table: "EntregasEPP",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequiereDevolucion",
                table: "ConfiguracionesMaterialEPP",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "VidaUtilDias",
                table: "ConfiguracionesMaterialEPP",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
