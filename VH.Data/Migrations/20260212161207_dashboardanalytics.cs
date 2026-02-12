using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VH.Data.Migrations
{
    /// <inheritdoc />
    public partial class dashboardanalytics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Justificacion",
                table: "RequisicionesEPP",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AddColumn<bool>(
                name: "Activo",
                table: "Proyectos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "EmpleadosEsperados",
                table: "Proyectos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaFinEstimada",
                table: "Proyectos",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PresupuestoEPPMensual",
                table: "Proyectos",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CategoriaRiesgo",
                table: "MaterialesEPP",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "EsDesechable",
                table: "MaterialesEPP",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "VidaUtilDiasDefault",
                table: "MaterialesEPP",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaIngreso",
                table: "Empleados",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaUltimoCalculoRiesgo",
                table: "Empleados",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdPuesto",
                table: "Empleados",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PuntuacionRiesgoActual",
                table: "Empleados",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "AlertasConsumo",
                columns: table => new
                {
                    IdAlerta = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TipoAlerta = table.Column<int>(type: "int", nullable: false),
                    Severidad = table.Column<int>(type: "int", nullable: false),
                    IdEmpleado = table.Column<int>(type: "int", nullable: false),
                    IdMaterial = table.Column<int>(type: "int", nullable: true),
                    IdProyecto = table.Column<int>(type: "int", nullable: true),
                    IdEntrega = table.Column<int>(type: "int", nullable: true),
                    IdRequisicion = table.Column<int>(type: "int", nullable: true),
                    Descripcion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ValorEsperado = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ValorReal = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Desviacion = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CostoEstimado = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    FechaGeneracion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaRevision = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IdUsuarioReviso = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    EstadoAlerta = table.Column<int>(type: "int", nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertasConsumo", x => x.IdAlerta);
                    table.ForeignKey(
                        name: "FK_AlertasConsumo_AspNetUsers_IdUsuarioReviso",
                        column: x => x.IdUsuarioReviso,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AlertasConsumo_Empleados_IdEmpleado",
                        column: x => x.IdEmpleado,
                        principalTable: "Empleados",
                        principalColumn: "IdEmpleado",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AlertasConsumo_EntregasEPP_IdEntrega",
                        column: x => x.IdEntrega,
                        principalTable: "EntregasEPP",
                        principalColumn: "IdEntrega");
                    table.ForeignKey(
                        name: "FK_AlertasConsumo_MaterialesEPP_IdMaterial",
                        column: x => x.IdMaterial,
                        principalTable: "MaterialesEPP",
                        principalColumn: "IdMaterial");
                    table.ForeignKey(
                        name: "FK_AlertasConsumo_Proyectos_IdProyecto",
                        column: x => x.IdProyecto,
                        principalTable: "Proyectos",
                        principalColumn: "IdProyecto");
                    table.ForeignKey(
                        name: "FK_AlertasConsumo_RequisicionesEPP_IdRequisicion",
                        column: x => x.IdRequisicion,
                        principalTable: "RequisicionesEPP",
                        principalColumn: "IdRequisicion");
                });

            migrationBuilder.CreateTable(
                name: "ConfiguracionesMaterialEPP",
                columns: table => new
                {
                    IdConfiguracion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdMaterial = table.Column<int>(type: "int", nullable: false),
                    VidaUtilDias = table.Column<int>(type: "int", nullable: false),
                    FrecuenciaMinimaDias = table.Column<int>(type: "int", nullable: false),
                    CantidadMaximaMensual = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CantidadMaximaPorEntrega = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    RequiereDevolucion = table.Column<bool>(type: "bit", nullable: false),
                    UmbralAlertaPorcentaje = table.Column<int>(type: "int", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracionesMaterialEPP", x => x.IdConfiguracion);
                    table.ForeignKey(
                        name: "FK_ConfiguracionesMaterialEPP_MaterialesEPP_IdMaterial",
                        column: x => x.IdMaterial,
                        principalTable: "MaterialesEPP",
                        principalColumn: "IdMaterial",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EstadisticasEmpleadoMensual",
                columns: table => new
                {
                    IdEstadistica = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdEmpleado = table.Column<int>(type: "int", nullable: false),
                    IdProyecto = table.Column<int>(type: "int", nullable: false),
                    Anio = table.Column<int>(type: "int", nullable: false),
                    Mes = table.Column<int>(type: "int", nullable: false),
                    TotalEntregas = table.Column<int>(type: "int", nullable: false),
                    TotalUnidades = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CostoTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaterialesDistintos = table.Column<int>(type: "int", nullable: false),
                    PromedioDesviacionVidaUtil = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AlertasGeneradas = table.Column<int>(type: "int", nullable: false),
                    PuntuacionRiesgo = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstadisticasEmpleadoMensual", x => x.IdEstadistica);
                    table.ForeignKey(
                        name: "FK_EstadisticasEmpleadoMensual_Empleados_IdEmpleado",
                        column: x => x.IdEmpleado,
                        principalTable: "Empleados",
                        principalColumn: "IdEmpleado",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EstadisticasEmpleadoMensual_Proyectos_IdProyecto",
                        column: x => x.IdProyecto,
                        principalTable: "Proyectos",
                        principalColumn: "IdProyecto",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EstadisticasProyectoMensual",
                columns: table => new
                {
                    IdEstadistica = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdProyecto = table.Column<int>(type: "int", nullable: false),
                    Anio = table.Column<int>(type: "int", nullable: false),
                    Mes = table.Column<int>(type: "int", nullable: false),
                    TotalEmpleados = table.Column<int>(type: "int", nullable: false),
                    TotalEntregas = table.Column<int>(type: "int", nullable: false),
                    TotalUnidades = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CostoTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CostoPromedioPorEmpleado = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PresupuestoAsignado = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DesviacionPresupuesto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AlertasCriticas = table.Column<int>(type: "int", nullable: false),
                    TotalAlertas = table.Column<int>(type: "int", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstadisticasProyectoMensual", x => x.IdEstadistica);
                    table.ForeignKey(
                        name: "FK_EstadisticasProyectoMensual_Proyectos_IdProyecto",
                        column: x => x.IdProyecto,
                        principalTable: "Proyectos",
                        principalColumn: "IdProyecto",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Puestos",
                columns: table => new
                {
                    IdPuesto = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    NivelRiesgoEPP = table.Column<int>(type: "int", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Puestos", x => x.IdPuesto);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Empleados_IdPuesto",
                table: "Empleados",
                column: "IdPuesto");

            migrationBuilder.CreateIndex(
                name: "IX_AlertasConsumo_IdEmpleado",
                table: "AlertasConsumo",
                column: "IdEmpleado");

            migrationBuilder.CreateIndex(
                name: "IX_AlertasConsumo_IdEntrega",
                table: "AlertasConsumo",
                column: "IdEntrega");

            migrationBuilder.CreateIndex(
                name: "IX_AlertasConsumo_IdMaterial",
                table: "AlertasConsumo",
                column: "IdMaterial");

            migrationBuilder.CreateIndex(
                name: "IX_AlertasConsumo_IdProyecto",
                table: "AlertasConsumo",
                column: "IdProyecto");

            migrationBuilder.CreateIndex(
                name: "IX_AlertasConsumo_IdRequisicion",
                table: "AlertasConsumo",
                column: "IdRequisicion");

            migrationBuilder.CreateIndex(
                name: "IX_AlertasConsumo_IdUsuarioReviso",
                table: "AlertasConsumo",
                column: "IdUsuarioReviso");

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionesMaterialEPP_IdMaterial",
                table: "ConfiguracionesMaterialEPP",
                column: "IdMaterial",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EstadisticasEmpleadoMensual_IdEmpleado",
                table: "EstadisticasEmpleadoMensual",
                column: "IdEmpleado");

            migrationBuilder.CreateIndex(
                name: "IX_EstadisticasEmpleadoMensual_IdProyecto",
                table: "EstadisticasEmpleadoMensual",
                column: "IdProyecto");

            migrationBuilder.CreateIndex(
                name: "IX_EstadisticasProyectoMensual_IdProyecto",
                table: "EstadisticasProyectoMensual",
                column: "IdProyecto");

            migrationBuilder.AddForeignKey(
                name: "FK_Empleados_Puestos_IdPuesto",
                table: "Empleados",
                column: "IdPuesto",
                principalTable: "Puestos",
                principalColumn: "IdPuesto");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Empleados_Puestos_IdPuesto",
                table: "Empleados");

            migrationBuilder.DropTable(
                name: "AlertasConsumo");

            migrationBuilder.DropTable(
                name: "ConfiguracionesMaterialEPP");

            migrationBuilder.DropTable(
                name: "EstadisticasEmpleadoMensual");

            migrationBuilder.DropTable(
                name: "EstadisticasProyectoMensual");

            migrationBuilder.DropTable(
                name: "Puestos");

            migrationBuilder.DropIndex(
                name: "IX_Empleados_IdPuesto",
                table: "Empleados");

            migrationBuilder.DropColumn(
                name: "Activo",
                table: "Proyectos");

            migrationBuilder.DropColumn(
                name: "EmpleadosEsperados",
                table: "Proyectos");

            migrationBuilder.DropColumn(
                name: "FechaFinEstimada",
                table: "Proyectos");

            migrationBuilder.DropColumn(
                name: "PresupuestoEPPMensual",
                table: "Proyectos");

            migrationBuilder.DropColumn(
                name: "CategoriaRiesgo",
                table: "MaterialesEPP");

            migrationBuilder.DropColumn(
                name: "EsDesechable",
                table: "MaterialesEPP");

            migrationBuilder.DropColumn(
                name: "VidaUtilDiasDefault",
                table: "MaterialesEPP");

            migrationBuilder.DropColumn(
                name: "FechaIngreso",
                table: "Empleados");

            migrationBuilder.DropColumn(
                name: "FechaUltimoCalculoRiesgo",
                table: "Empleados");

            migrationBuilder.DropColumn(
                name: "IdPuesto",
                table: "Empleados");

            migrationBuilder.DropColumn(
                name: "PuntuacionRiesgoActual",
                table: "Empleados");

            migrationBuilder.AlterColumn<string>(
                name: "Justificacion",
                table: "RequisicionesEPP",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);
        }
    }
}
