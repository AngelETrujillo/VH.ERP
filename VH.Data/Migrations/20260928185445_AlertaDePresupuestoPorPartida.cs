using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VH.Data.Migrations
{
    /// <inheritdoc />
    public partial class AlertaDePresupuestoPorPartida : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "IdEmpleado",
                table: "AlertasConsumo",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "IdPartida",
                table: "AlertasConsumo",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AlertasConsumo_IdPartida",
                table: "AlertasConsumo",
                column: "IdPartida");

            migrationBuilder.AddForeignKey(
                name: "FK_AlertasConsumo_ConceptosPartidas_IdPartida",
                table: "AlertasConsumo",
                column: "IdPartida",
                principalTable: "ConceptosPartidas",
                principalColumn: "IdPartida",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AlertasConsumo_ConceptosPartidas_IdPartida",
                table: "AlertasConsumo");

            migrationBuilder.DropIndex(
                name: "IX_AlertasConsumo_IdPartida",
                table: "AlertasConsumo");

            migrationBuilder.DropColumn(
                name: "IdPartida",
                table: "AlertasConsumo");

            migrationBuilder.AlterColumn<int>(
                name: "IdEmpleado",
                table: "AlertasConsumo",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
