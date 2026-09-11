using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VH.Data.Migrations
{
    /// <summary>
    /// Generaliza el catálogo de EPP a un catálogo de materiales que también admite
    /// consumibles y herramienta.
    ///
    /// EF generó originalmente esta migración como DROP TABLE MaterialesEPP +
    /// CREATE TABLE Materiales, porque no reconoce el cambio de nombre y lo trata
    /// como una tabla distinta. Eso habría borrado el catálogo completo y, con él,
    /// las compras, inventarios y entregas que lo referencian. Se reescribió a mano
    /// como un rename real, que conserva los datos.
    /// </summary>
    public partial class CatalogoMaterialGeneralizado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Soltar las claves foráneas que apuntan a la tabla, para poder renombrarla
            //    y volver a crearlas con el nombre nuevo.
            migrationBuilder.DropForeignKey(
                name: "FK_AlertasConsumo_MaterialesEPP_IdMaterial",
                table: "AlertasConsumo");

            migrationBuilder.DropForeignKey(
                name: "FK_ComprasEPP_MaterialesEPP_IdMaterial",
                table: "ComprasEPP");

            migrationBuilder.DropForeignKey(
                name: "FK_ConfiguracionesMaterialEPP_MaterialesEPP_IdMaterial",
                table: "ConfiguracionesMaterialEPP");

            migrationBuilder.DropForeignKey(
                name: "FK_Inventarios_MaterialesEPP_IdMaterial",
                table: "Inventarios");

            migrationBuilder.DropForeignKey(
                name: "FK_RequisicionesEPPDetalle_MaterialesEPP_IdMaterial",
                table: "RequisicionesEPPDetalle");

            // 2. Renombrar la tabla conservando sus filas.
            migrationBuilder.RenameTable(
                name: "MaterialesEPP",
                newName: "Materiales");

            // 3. sp_rename conserva los nombres viejos de índice y clave primaria;
            //    se alinean para que coincidan con el modelo.
            migrationBuilder.RenameIndex(
                name: "IX_MaterialesEPP_IdUnidadMedida",
                table: "Materiales",
                newName: "IX_Materiales_IdUnidadMedida");

            migrationBuilder.Sql("EXEC sp_rename N'PK_MaterialesEPP', N'PK_Materiales', N'OBJECT';");

            migrationBuilder.Sql(
                "EXEC sp_rename N'FK_MaterialesEPP_UnidadesMedida_IdUnidadMedida', " +
                "N'FK_Materiales_UnidadesMedida_IdUnidadMedida', N'OBJECT';");

            // 4. Clasificación. Todo lo que existe hoy es equipo de protección personal,
            //    se entrega por talla y no se devuelve.
            migrationBuilder.AddColumn<int>(
                name: "TipoMaterial",
                table: "Materiales",
                type: "int",
                nullable: false,
                defaultValue: 1);          // TipoMaterial.EPP

            migrationBuilder.AddColumn<bool>(
                name: "EsRetornable",
                table: "Materiales",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiereTalla",
                table: "Materiales",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "ControlaCaducidad",
                table: "Materiales",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Materiales_TipoMaterial",
                table: "Materiales",
                column: "TipoMaterial");

            // 5. Rehacer las claves foráneas contra el nombre nuevo.
            migrationBuilder.AddForeignKey(
                name: "FK_AlertasConsumo_Materiales_IdMaterial",
                table: "AlertasConsumo",
                column: "IdMaterial",
                principalTable: "Materiales",
                principalColumn: "IdMaterial",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ComprasEPP_Materiales_IdMaterial",
                table: "ComprasEPP",
                column: "IdMaterial",
                principalTable: "Materiales",
                principalColumn: "IdMaterial",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ConfiguracionesMaterialEPP_Materiales_IdMaterial",
                table: "ConfiguracionesMaterialEPP",
                column: "IdMaterial",
                principalTable: "Materiales",
                principalColumn: "IdMaterial",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Inventarios_Materiales_IdMaterial",
                table: "Inventarios",
                column: "IdMaterial",
                principalTable: "Materiales",
                principalColumn: "IdMaterial",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RequisicionesEPPDetalle_Materiales_IdMaterial",
                table: "RequisicionesEPPDetalle",
                column: "IdMaterial",
                principalTable: "Materiales",
                principalColumn: "IdMaterial",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AlertasConsumo_Materiales_IdMaterial",
                table: "AlertasConsumo");

            migrationBuilder.DropForeignKey(
                name: "FK_ComprasEPP_Materiales_IdMaterial",
                table: "ComprasEPP");

            migrationBuilder.DropForeignKey(
                name: "FK_ConfiguracionesMaterialEPP_Materiales_IdMaterial",
                table: "ConfiguracionesMaterialEPP");

            migrationBuilder.DropForeignKey(
                name: "FK_Inventarios_Materiales_IdMaterial",
                table: "Inventarios");

            migrationBuilder.DropForeignKey(
                name: "FK_RequisicionesEPPDetalle_Materiales_IdMaterial",
                table: "RequisicionesEPPDetalle");

            migrationBuilder.DropIndex(
                name: "IX_Materiales_TipoMaterial",
                table: "Materiales");

            migrationBuilder.DropColumn(name: "TipoMaterial", table: "Materiales");
            migrationBuilder.DropColumn(name: "EsRetornable", table: "Materiales");
            migrationBuilder.DropColumn(name: "RequiereTalla", table: "Materiales");
            migrationBuilder.DropColumn(name: "ControlaCaducidad", table: "Materiales");

            migrationBuilder.Sql(
                "EXEC sp_rename N'FK_Materiales_UnidadesMedida_IdUnidadMedida', " +
                "N'FK_MaterialesEPP_UnidadesMedida_IdUnidadMedida', N'OBJECT';");

            migrationBuilder.Sql("EXEC sp_rename N'PK_Materiales', N'PK_MaterialesEPP', N'OBJECT';");

            migrationBuilder.RenameIndex(
                name: "IX_Materiales_IdUnidadMedida",
                table: "Materiales",
                newName: "IX_MaterialesEPP_IdUnidadMedida");

            migrationBuilder.RenameTable(
                name: "Materiales",
                newName: "MaterialesEPP");

            migrationBuilder.AddForeignKey(
                name: "FK_AlertasConsumo_MaterialesEPP_IdMaterial",
                table: "AlertasConsumo",
                column: "IdMaterial",
                principalTable: "MaterialesEPP",
                principalColumn: "IdMaterial",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ComprasEPP_MaterialesEPP_IdMaterial",
                table: "ComprasEPP",
                column: "IdMaterial",
                principalTable: "MaterialesEPP",
                principalColumn: "IdMaterial",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ConfiguracionesMaterialEPP_MaterialesEPP_IdMaterial",
                table: "ConfiguracionesMaterialEPP",
                column: "IdMaterial",
                principalTable: "MaterialesEPP",
                principalColumn: "IdMaterial",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Inventarios_MaterialesEPP_IdMaterial",
                table: "Inventarios",
                column: "IdMaterial",
                principalTable: "MaterialesEPP",
                principalColumn: "IdMaterial",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RequisicionesEPPDetalle_MaterialesEPP_IdMaterial",
                table: "RequisicionesEPPDetalle",
                column: "IdMaterial",
                principalTable: "MaterialesEPP",
                principalColumn: "IdMaterial",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
