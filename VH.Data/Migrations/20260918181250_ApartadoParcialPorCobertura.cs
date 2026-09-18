using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VH.Data.Migrations
{
    /// <inheritdoc />
    public partial class ApartadoParcialPorCobertura : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CantidadApartada",
                table: "RequisicionesCobertura",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            // Hasta hoy una cobertura sólo se apartaba completa o no se apartaba:
            // no existían las mitades. Así que todo lo que ya está recibido o
            // surtido llegó entero, y hay que decirlo. Dejarlo en cero haría creer
            // al sistema que ese material nunca llegó, y volvería a apartarlo en la
            // siguiente recepción.
            migrationBuilder.Sql(@"
                UPDATE c
                SET c.CantidadApartada = c.Cantidad
                FROM RequisicionesCobertura c
                INNER JOIN RequisicionesEPPDetalle d
                    ON d.IdRequisicionDetalle = c.IdRequisicionDetalle
                WHERE d.EstadoRenglon IN (3, 8);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CantidadApartada",
                table: "RequisicionesCobertura");
        }
    }
}
