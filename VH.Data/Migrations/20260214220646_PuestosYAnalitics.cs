using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VH.Data.Migrations
{
    /// <inheritdoc />
    public partial class PuestosYAnalitics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AlertasConsumo_AspNetUsers_IdUsuarioReviso",
                table: "AlertasConsumo");

            migrationBuilder.DropForeignKey(
                name: "FK_AlertasConsumo_Empleados_IdEmpleado",
                table: "AlertasConsumo");

            migrationBuilder.DropForeignKey(
                name: "FK_AlertasConsumo_EntregasEPP_IdEntrega",
                table: "AlertasConsumo");

            migrationBuilder.DropForeignKey(
                name: "FK_AlertasConsumo_MaterialesEPP_IdMaterial",
                table: "AlertasConsumo");

            migrationBuilder.DropForeignKey(
                name: "FK_AlertasConsumo_Proyectos_IdProyecto",
                table: "AlertasConsumo");

            migrationBuilder.DropForeignKey(
                name: "FK_AlertasConsumo_RequisicionesEPP_IdRequisicion",
                table: "AlertasConsumo");

            migrationBuilder.DropForeignKey(
                name: "FK_ConfiguracionesMaterialEPP_MaterialesEPP_IdMaterial",
                table: "ConfiguracionesMaterialEPP");

            migrationBuilder.DropForeignKey(
                name: "FK_Empleados_Puestos_IdPuesto",
                table: "Empleados");

            migrationBuilder.DropForeignKey(
                name: "FK_EstadisticasEmpleadoMensual_Empleados_IdEmpleado",
                table: "EstadisticasEmpleadoMensual");

            migrationBuilder.DropForeignKey(
                name: "FK_EstadisticasEmpleadoMensual_Proyectos_IdProyecto",
                table: "EstadisticasEmpleadoMensual");

            migrationBuilder.DropForeignKey(
                name: "FK_EstadisticasProyectoMensual_Proyectos_IdProyecto",
                table: "EstadisticasProyectoMensual");

            migrationBuilder.DropIndex(
                name: "IX_UnidadesMedida_Abreviatura",
                table: "UnidadesMedida");

            migrationBuilder.DropIndex(
                name: "IX_UnidadesMedida_Nombre",
                table: "UnidadesMedida");

            migrationBuilder.DropIndex(
                name: "IX_RequisicionesEPPDetalle_IdRequisicion_IdMaterial",
                table: "RequisicionesEPPDetalle");

            migrationBuilder.DropIndex(
                name: "IX_EstadisticasProyectoMensual_IdProyecto",
                table: "EstadisticasProyectoMensual");

            migrationBuilder.DropIndex(
                name: "IX_EstadisticasEmpleadoMensual_IdEmpleado",
                table: "EstadisticasEmpleadoMensual");

            migrationBuilder.DropIndex(
                name: "IX_EstadisticasEmpleadoMensual_IdProyecto",
                table: "EstadisticasEmpleadoMensual");

            migrationBuilder.DropIndex(
                name: "IX_ComprasEPP_IdMaterial_IdAlmacen",
                table: "ComprasEPP");

            migrationBuilder.AlterColumn<string>(
                name: "Descripcion",
                table: "UnidadesMedida",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AddColumn<int>(
                name: "ModuloIdModulo",
                table: "RolPermisos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RolId",
                table: "RolPermisos",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Descripcion",
                table: "Modulos",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalUnidades",
                table: "EstadisticasProyectoMensual",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalUnidades",
                table: "EstadisticasEmpleadoMensual",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "CantidadMaximaPorEntrega",
                table: "ConfiguracionesMaterialEPP",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "CantidadMaximaMensual",
                table: "ConfiguracionesMaterialEPP",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RolPermisos_ModuloIdModulo",
                table: "RolPermisos",
                column: "ModuloIdModulo");

            migrationBuilder.CreateIndex(
                name: "IX_RolPermisos_RolId",
                table: "RolPermisos",
                column: "RolId");

            migrationBuilder.CreateIndex(
                name: "IX_RequisicionesEPPDetalle_IdRequisicion",
                table: "RequisicionesEPPDetalle",
                column: "IdRequisicion");

            migrationBuilder.CreateIndex(
                name: "IX_Puestos_Nombre",
                table: "Puestos",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EstadisticasProyectoMensual_IdProyecto_Anio_Mes",
                table: "EstadisticasProyectoMensual",
                columns: new[] { "IdProyecto", "Anio", "Mes" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EstadisticasEmpleadoMensual_IdEmpleado_Anio_Mes",
                table: "EstadisticasEmpleadoMensual",
                columns: new[] { "IdEmpleado", "Anio", "Mes" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EstadisticasEmpleadoMensual_IdProyecto_Anio_Mes",
                table: "EstadisticasEmpleadoMensual",
                columns: new[] { "IdProyecto", "Anio", "Mes" });

            migrationBuilder.CreateIndex(
                name: "IX_EstadisticasEmpleadoMensual_PuntuacionRiesgo",
                table: "EstadisticasEmpleadoMensual",
                column: "PuntuacionRiesgo");

            migrationBuilder.CreateIndex(
                name: "IX_ComprasEPP_IdMaterial",
                table: "ComprasEPP",
                column: "IdMaterial");

            migrationBuilder.CreateIndex(
                name: "IX_AlertasConsumo_EstadoAlerta",
                table: "AlertasConsumo",
                column: "EstadoAlerta");

            migrationBuilder.CreateIndex(
                name: "IX_AlertasConsumo_FechaGeneracion",
                table: "AlertasConsumo",
                column: "FechaGeneracion");

            migrationBuilder.CreateIndex(
                name: "IX_AlertasConsumo_Severidad",
                table: "AlertasConsumo",
                column: "Severidad");

            migrationBuilder.AddForeignKey(
                name: "FK_AlertasConsumo_AspNetUsers_IdUsuarioReviso",
                table: "AlertasConsumo",
                column: "IdUsuarioReviso",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AlertasConsumo_Empleados_IdEmpleado",
                table: "AlertasConsumo",
                column: "IdEmpleado",
                principalTable: "Empleados",
                principalColumn: "IdEmpleado",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AlertasConsumo_EntregasEPP_IdEntrega",
                table: "AlertasConsumo",
                column: "IdEntrega",
                principalTable: "EntregasEPP",
                principalColumn: "IdEntrega",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AlertasConsumo_MaterialesEPP_IdMaterial",
                table: "AlertasConsumo",
                column: "IdMaterial",
                principalTable: "MaterialesEPP",
                principalColumn: "IdMaterial",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AlertasConsumo_Proyectos_IdProyecto",
                table: "AlertasConsumo",
                column: "IdProyecto",
                principalTable: "Proyectos",
                principalColumn: "IdProyecto",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AlertasConsumo_RequisicionesEPP_IdRequisicion",
                table: "AlertasConsumo",
                column: "IdRequisicion",
                principalTable: "RequisicionesEPP",
                principalColumn: "IdRequisicion",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ConfiguracionesMaterialEPP_MaterialesEPP_IdMaterial",
                table: "ConfiguracionesMaterialEPP",
                column: "IdMaterial",
                principalTable: "MaterialesEPP",
                principalColumn: "IdMaterial",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Empleados_Puestos_IdPuesto",
                table: "Empleados",
                column: "IdPuesto",
                principalTable: "Puestos",
                principalColumn: "IdPuesto",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EstadisticasEmpleadoMensual_Empleados_IdEmpleado",
                table: "EstadisticasEmpleadoMensual",
                column: "IdEmpleado",
                principalTable: "Empleados",
                principalColumn: "IdEmpleado",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EstadisticasEmpleadoMensual_Proyectos_IdProyecto",
                table: "EstadisticasEmpleadoMensual",
                column: "IdProyecto",
                principalTable: "Proyectos",
                principalColumn: "IdProyecto",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EstadisticasProyectoMensual_Proyectos_IdProyecto",
                table: "EstadisticasProyectoMensual",
                column: "IdProyecto",
                principalTable: "Proyectos",
                principalColumn: "IdProyecto",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RolPermisos_AspNetRoles_RolId",
                table: "RolPermisos",
                column: "RolId",
                principalTable: "AspNetRoles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RolPermisos_Modulos_ModuloIdModulo",
                table: "RolPermisos",
                column: "ModuloIdModulo",
                principalTable: "Modulos",
                principalColumn: "IdModulo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AlertasConsumo_AspNetUsers_IdUsuarioReviso",
                table: "AlertasConsumo");

            migrationBuilder.DropForeignKey(
                name: "FK_AlertasConsumo_Empleados_IdEmpleado",
                table: "AlertasConsumo");

            migrationBuilder.DropForeignKey(
                name: "FK_AlertasConsumo_EntregasEPP_IdEntrega",
                table: "AlertasConsumo");

            migrationBuilder.DropForeignKey(
                name: "FK_AlertasConsumo_MaterialesEPP_IdMaterial",
                table: "AlertasConsumo");

            migrationBuilder.DropForeignKey(
                name: "FK_AlertasConsumo_Proyectos_IdProyecto",
                table: "AlertasConsumo");

            migrationBuilder.DropForeignKey(
                name: "FK_AlertasConsumo_RequisicionesEPP_IdRequisicion",
                table: "AlertasConsumo");

            migrationBuilder.DropForeignKey(
                name: "FK_ConfiguracionesMaterialEPP_MaterialesEPP_IdMaterial",
                table: "ConfiguracionesMaterialEPP");

            migrationBuilder.DropForeignKey(
                name: "FK_Empleados_Puestos_IdPuesto",
                table: "Empleados");

            migrationBuilder.DropForeignKey(
                name: "FK_EstadisticasEmpleadoMensual_Empleados_IdEmpleado",
                table: "EstadisticasEmpleadoMensual");

            migrationBuilder.DropForeignKey(
                name: "FK_EstadisticasEmpleadoMensual_Proyectos_IdProyecto",
                table: "EstadisticasEmpleadoMensual");

            migrationBuilder.DropForeignKey(
                name: "FK_EstadisticasProyectoMensual_Proyectos_IdProyecto",
                table: "EstadisticasProyectoMensual");

            migrationBuilder.DropForeignKey(
                name: "FK_RolPermisos_AspNetRoles_RolId",
                table: "RolPermisos");

            migrationBuilder.DropForeignKey(
                name: "FK_RolPermisos_Modulos_ModuloIdModulo",
                table: "RolPermisos");

            migrationBuilder.DropIndex(
                name: "IX_RolPermisos_ModuloIdModulo",
                table: "RolPermisos");

            migrationBuilder.DropIndex(
                name: "IX_RolPermisos_RolId",
                table: "RolPermisos");

            migrationBuilder.DropIndex(
                name: "IX_RequisicionesEPPDetalle_IdRequisicion",
                table: "RequisicionesEPPDetalle");

            migrationBuilder.DropIndex(
                name: "IX_Puestos_Nombre",
                table: "Puestos");

            migrationBuilder.DropIndex(
                name: "IX_EstadisticasProyectoMensual_IdProyecto_Anio_Mes",
                table: "EstadisticasProyectoMensual");

            migrationBuilder.DropIndex(
                name: "IX_EstadisticasEmpleadoMensual_IdEmpleado_Anio_Mes",
                table: "EstadisticasEmpleadoMensual");

            migrationBuilder.DropIndex(
                name: "IX_EstadisticasEmpleadoMensual_IdProyecto_Anio_Mes",
                table: "EstadisticasEmpleadoMensual");

            migrationBuilder.DropIndex(
                name: "IX_EstadisticasEmpleadoMensual_PuntuacionRiesgo",
                table: "EstadisticasEmpleadoMensual");

            migrationBuilder.DropIndex(
                name: "IX_ComprasEPP_IdMaterial",
                table: "ComprasEPP");

            migrationBuilder.DropIndex(
                name: "IX_AlertasConsumo_EstadoAlerta",
                table: "AlertasConsumo");

            migrationBuilder.DropIndex(
                name: "IX_AlertasConsumo_FechaGeneracion",
                table: "AlertasConsumo");

            migrationBuilder.DropIndex(
                name: "IX_AlertasConsumo_Severidad",
                table: "AlertasConsumo");

            migrationBuilder.DropColumn(
                name: "ModuloIdModulo",
                table: "RolPermisos");

            migrationBuilder.DropColumn(
                name: "RolId",
                table: "RolPermisos");

            migrationBuilder.AlterColumn<string>(
                name: "Descripcion",
                table: "UnidadesMedida",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Descripcion",
                table: "Modulos",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalUnidades",
                table: "EstadisticasProyectoMensual",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalUnidades",
                table: "EstadisticasEmpleadoMensual",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AlterColumn<decimal>(
                name: "CantidadMaximaPorEntrega",
                table: "ConfiguracionesMaterialEPP",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "CantidadMaximaMensual",
                table: "ConfiguracionesMaterialEPP",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UnidadesMedida_Abreviatura",
                table: "UnidadesMedida",
                column: "Abreviatura",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UnidadesMedida_Nombre",
                table: "UnidadesMedida",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RequisicionesEPPDetalle_IdRequisicion_IdMaterial",
                table: "RequisicionesEPPDetalle",
                columns: new[] { "IdRequisicion", "IdMaterial" });

            migrationBuilder.CreateIndex(
                name: "IX_EstadisticasProyectoMensual_IdProyecto",
                table: "EstadisticasProyectoMensual",
                column: "IdProyecto");

            migrationBuilder.CreateIndex(
                name: "IX_EstadisticasEmpleadoMensual_IdEmpleado",
                table: "EstadisticasEmpleadoMensual",
                column: "IdEmpleado");

            migrationBuilder.CreateIndex(
                name: "IX_EstadisticasEmpleadoMensual_IdProyecto",
                table: "EstadisticasEmpleadoMensual",
                column: "IdProyecto");

            migrationBuilder.CreateIndex(
                name: "IX_ComprasEPP_IdMaterial_IdAlmacen",
                table: "ComprasEPP",
                columns: new[] { "IdMaterial", "IdAlmacen" });

            migrationBuilder.AddForeignKey(
                name: "FK_AlertasConsumo_AspNetUsers_IdUsuarioReviso",
                table: "AlertasConsumo",
                column: "IdUsuarioReviso",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AlertasConsumo_Empleados_IdEmpleado",
                table: "AlertasConsumo",
                column: "IdEmpleado",
                principalTable: "Empleados",
                principalColumn: "IdEmpleado",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AlertasConsumo_EntregasEPP_IdEntrega",
                table: "AlertasConsumo",
                column: "IdEntrega",
                principalTable: "EntregasEPP",
                principalColumn: "IdEntrega");

            migrationBuilder.AddForeignKey(
                name: "FK_AlertasConsumo_MaterialesEPP_IdMaterial",
                table: "AlertasConsumo",
                column: "IdMaterial",
                principalTable: "MaterialesEPP",
                principalColumn: "IdMaterial");

            migrationBuilder.AddForeignKey(
                name: "FK_AlertasConsumo_Proyectos_IdProyecto",
                table: "AlertasConsumo",
                column: "IdProyecto",
                principalTable: "Proyectos",
                principalColumn: "IdProyecto");

            migrationBuilder.AddForeignKey(
                name: "FK_AlertasConsumo_RequisicionesEPP_IdRequisicion",
                table: "AlertasConsumo",
                column: "IdRequisicion",
                principalTable: "RequisicionesEPP",
                principalColumn: "IdRequisicion");

            migrationBuilder.AddForeignKey(
                name: "FK_ConfiguracionesMaterialEPP_MaterialesEPP_IdMaterial",
                table: "ConfiguracionesMaterialEPP",
                column: "IdMaterial",
                principalTable: "MaterialesEPP",
                principalColumn: "IdMaterial",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Empleados_Puestos_IdPuesto",
                table: "Empleados",
                column: "IdPuesto",
                principalTable: "Puestos",
                principalColumn: "IdPuesto");

            migrationBuilder.AddForeignKey(
                name: "FK_EstadisticasEmpleadoMensual_Empleados_IdEmpleado",
                table: "EstadisticasEmpleadoMensual",
                column: "IdEmpleado",
                principalTable: "Empleados",
                principalColumn: "IdEmpleado",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EstadisticasEmpleadoMensual_Proyectos_IdProyecto",
                table: "EstadisticasEmpleadoMensual",
                column: "IdProyecto",
                principalTable: "Proyectos",
                principalColumn: "IdProyecto",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EstadisticasProyectoMensual_Proyectos_IdProyecto",
                table: "EstadisticasProyectoMensual",
                column: "IdProyecto",
                principalTable: "Proyectos",
                principalColumn: "IdProyecto",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
