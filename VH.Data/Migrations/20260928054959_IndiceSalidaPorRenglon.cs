using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VH.Data.Migrations
{
    /// <inheritdoc />
    public partial class IndiceSalidaPorRenglon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EntregasEPP_RequisicionesEPPDetalle_IdRequisicionDetalle",
                table: "EntregasEPP");

            migrationBuilder.AddForeignKey(
                name: "FK_EntregasEPP_RequisicionesEPPDetalle_IdRequisicionDetalle",
                table: "EntregasEPP",
                column: "IdRequisicionDetalle",
                principalTable: "RequisicionesEPPDetalle",
                principalColumn: "IdRequisicionDetalle",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EntregasEPP_RequisicionesEPPDetalle_IdRequisicionDetalle",
                table: "EntregasEPP");

            migrationBuilder.AddForeignKey(
                name: "FK_EntregasEPP_RequisicionesEPPDetalle_IdRequisicionDetalle",
                table: "EntregasEPP",
                column: "IdRequisicionDetalle",
                principalTable: "RequisicionesEPPDetalle",
                principalColumn: "IdRequisicionDetalle");
        }
    }
}
