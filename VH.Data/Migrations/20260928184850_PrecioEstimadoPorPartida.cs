using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VH.Data.Migrations
{
    /// <inheritdoc />
    public partial class PrecioEstimadoPorPartida : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PrecioUnitarioEstimado",
                table: "ConceptosPartidas",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrecioUnitarioEstimado",
                table: "ConceptosPartidas");
        }
    }
}
