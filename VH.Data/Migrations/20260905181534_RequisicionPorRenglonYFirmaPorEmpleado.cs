using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VH.Data.Migrations
{
    /// <summary>
    /// Baja el destinatario y el estado al renglón de la requisición, y saca la
    /// firma de la cabecera a una entrega por persona.
    ///
    /// EF generó esta migración soltando IdEmpleadoRecibe, FirmaDigital,
    /// FotoEvidencia, IdUsuarioEntrega y FechaEntrega antes de copiar nada, lo que
    /// habría dejado los renglones sin destinatario y habría borrado las firmas
    /// existentes, que son el respaldo probatorio de las entregas ya hechas.
    /// Se reescribió a mano para copiar primero.
    /// </summary>
    public partial class RequisicionPorRenglonYFirmaPorEmpleado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── 1. Columnas nuevas del renglón ───────────────────────────────
            migrationBuilder.AddColumn<int>(
                name: "EstadoRenglon",
                table: "RequisicionesEPPDetalle",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "IdEmpleadoDestino",
                table: "RequisicionesEPPDetalle",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdProyectoDestino",
                table: "RequisicionesEPPDetalle",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdConceptoPartida",
                table: "RequisicionesEPPDetalle",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotivoRechazo",
                table: "RequisicionesEPPDetalle",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaRequerida",
                table: "RequisicionesEPP",
                type: "datetime2",
                nullable: true);

            // ── 2. Tabla de entregas firmadas ────────────────────────────────
            migrationBuilder.CreateTable(
                name: "RequisicionesEntregas",
                columns: table => new
                {
                    IdRequisicionEntrega = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdRequisicion = table.Column<int>(type: "int", nullable: false),
                    IdEmpleado = table.Column<int>(type: "int", nullable: false),
                    FechaEntrega = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IdUsuarioEntrega = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FirmaDigital = table.Column<string>(type: "nvarchar(max)", maxLength: 500000, nullable: false),
                    FotoEvidencia = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequisicionesEntregas", x => x.IdRequisicionEntrega);
                    table.ForeignKey(
                        name: "FK_RequisicionesEntregas_AspNetUsers_IdUsuarioEntrega",
                        column: x => x.IdUsuarioEntrega,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RequisicionesEntregas_Empleados_IdEmpleado",
                        column: x => x.IdEmpleado,
                        principalTable: "Empleados",
                        principalColumn: "IdEmpleado",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RequisicionesEntregas_RequisicionesEPP_IdRequisicion",
                        column: x => x.IdRequisicion,
                        principalTable: "RequisicionesEPP",
                        principalColumn: "IdRequisicion",
                        onDelete: ReferentialAction.Cascade);
                });

            // ── 3. Copiar los datos antes de soltar las columnas ─────────────

            // El destinatario del documento pasa a ser el de todos sus renglones:
            // hasta ahora una requisición era siempre de una sola persona.
            migrationBuilder.Sql(@"
                UPDATE d
                SET d.[IdEmpleadoDestino] = r.[IdEmpleadoRecibe]
                FROM [RequisicionesEPPDetalle] d
                INNER JOIN [RequisicionesEPP] r ON r.[IdRequisicion] = d.[IdRequisicion];
            ");

            // El estado del documento se reparte a sus renglones.
            migrationBuilder.Sql(@"
                UPDATE d
                SET d.[EstadoRenglon] =
                    CASE r.[EstadoRequisicion]
                        WHEN 0 THEN 0   -- Pendiente  -> Solicitado
                        WHEN 1 THEN 1   -- Aprobada   -> Autorizado
                        WHEN 2 THEN 2   -- Rechazada  -> Rechazado
                        WHEN 3 THEN 3   -- Entregada  -> Surtido
                        WHEN 4 THEN 4   -- Cancelada  -> Cancelado
                        ELSE 0
                    END,
                    d.[MotivoRechazo] = CASE WHEN r.[EstadoRequisicion] = 2 THEN r.[MotivoRechazo] ELSE NULL END
                FROM [RequisicionesEPPDetalle] d
                INNER JOIN [RequisicionesEPP] r ON r.[IdRequisicion] = d.[IdRequisicion];
            ");

            // Cada firma existente se convierte en la entrega de esa persona.
            // Sin firma no hay nada que conservar: la entrega nunca se registró.
            migrationBuilder.Sql(@"
                INSERT INTO [RequisicionesEntregas]
                    ([IdRequisicion], [IdEmpleado], [FechaEntrega], [IdUsuarioEntrega],
                     [FirmaDigital], [FotoEvidencia], [Observaciones])
                SELECT
                    r.[IdRequisicion],
                    r.[IdEmpleadoRecibe],
                    ISNULL(r.[FechaEntrega], r.[FechaSolicitud]),
                    ISNULL(r.[IdUsuarioEntrega], r.[IdUsuarioSolicita]),
                    r.[FirmaDigital],
                    r.[FotoEvidencia],
                    r.[Observaciones]
                FROM [RequisicionesEPP] r
                WHERE r.[FirmaDigital] IS NOT NULL AND LEN(r.[FirmaDigital]) > 0;
            ");

            // ── 4. Soltar lo que ya se copió ─────────────────────────────────
            migrationBuilder.DropForeignKey(
                name: "FK_RequisicionesEPP_AspNetUsers_IdUsuarioEntrega",
                table: "RequisicionesEPP");

            migrationBuilder.DropForeignKey(
                name: "FK_RequisicionesEPP_Empleados_IdEmpleadoRecibe",
                table: "RequisicionesEPP");

            migrationBuilder.DropIndex(
                name: "IX_RequisicionesEPP_IdEmpleadoRecibe",
                table: "RequisicionesEPP");

            migrationBuilder.DropIndex(
                name: "IX_RequisicionesEPP_IdUsuarioEntrega",
                table: "RequisicionesEPP");

            migrationBuilder.DropColumn(name: "FirmaDigital", table: "RequisicionesEPP");
            migrationBuilder.DropColumn(name: "FotoEvidencia", table: "RequisicionesEPP");
            migrationBuilder.DropColumn(name: "IdEmpleadoRecibe", table: "RequisicionesEPP");
            migrationBuilder.DropColumn(name: "IdUsuarioEntrega", table: "RequisicionesEPP");
            migrationBuilder.DropColumn(name: "Observaciones", table: "RequisicionesEPP");
            migrationBuilder.DropColumn(name: "FechaEntrega", table: "RequisicionesEPP");

            // ── 5. Índices y claves foráneas nuevas ──────────────────────────
            migrationBuilder.CreateIndex(
                name: "IX_RequisicionesEPPDetalle_EstadoRenglon",
                table: "RequisicionesEPPDetalle",
                column: "EstadoRenglon");

            migrationBuilder.CreateIndex(
                name: "IX_RequisicionesEPPDetalle_IdConceptoPartida",
                table: "RequisicionesEPPDetalle",
                column: "IdConceptoPartida");

            migrationBuilder.CreateIndex(
                name: "IX_RequisicionesEPPDetalle_IdEmpleadoDestino",
                table: "RequisicionesEPPDetalle",
                column: "IdEmpleadoDestino");

            migrationBuilder.CreateIndex(
                name: "IX_RequisicionesEPPDetalle_IdProyectoDestino",
                table: "RequisicionesEPPDetalle",
                column: "IdProyectoDestino");

            migrationBuilder.CreateIndex(
                name: "IX_RequisicionesEntregas_FechaEntrega",
                table: "RequisicionesEntregas",
                column: "FechaEntrega");

            migrationBuilder.CreateIndex(
                name: "IX_RequisicionesEntregas_IdEmpleado",
                table: "RequisicionesEntregas",
                column: "IdEmpleado");

            migrationBuilder.CreateIndex(
                name: "IX_RequisicionesEntregas_IdRequisicion_IdEmpleado",
                table: "RequisicionesEntregas",
                columns: new[] { "IdRequisicion", "IdEmpleado" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RequisicionesEntregas_IdUsuarioEntrega",
                table: "RequisicionesEntregas",
                column: "IdUsuarioEntrega");

            migrationBuilder.AddForeignKey(
                name: "FK_RequisicionesEPPDetalle_ConceptosPartidas_IdConceptoPartida",
                table: "RequisicionesEPPDetalle",
                column: "IdConceptoPartida",
                principalTable: "ConceptosPartidas",
                principalColumn: "IdPartida",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RequisicionesEPPDetalle_Empleados_IdEmpleadoDestino",
                table: "RequisicionesEPPDetalle",
                column: "IdEmpleadoDestino",
                principalTable: "Empleados",
                principalColumn: "IdEmpleado",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RequisicionesEPPDetalle_Proyectos_IdProyectoDestino",
                table: "RequisicionesEPPDetalle",
                column: "IdProyectoDestino",
                principalTable: "Proyectos",
                principalColumn: "IdProyecto",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RequisicionesEPPDetalle_ConceptosPartidas_IdConceptoPartida",
                table: "RequisicionesEPPDetalle");

            migrationBuilder.DropForeignKey(
                name: "FK_RequisicionesEPPDetalle_Empleados_IdEmpleadoDestino",
                table: "RequisicionesEPPDetalle");

            migrationBuilder.DropForeignKey(
                name: "FK_RequisicionesEPPDetalle_Proyectos_IdProyectoDestino",
                table: "RequisicionesEPPDetalle");

            // Devolver las columnas a la cabecera.
            migrationBuilder.AddColumn<int>(
                name: "IdEmpleadoRecibe",
                table: "RequisicionesEPP",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "FirmaDigital",
                table: "RequisicionesEPP",
                type: "nvarchar(max)",
                maxLength: 500000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FotoEvidencia",
                table: "RequisicionesEPP",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdUsuarioEntrega",
                table: "RequisicionesEPP",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Observaciones",
                table: "RequisicionesEPP",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaEntrega",
                table: "RequisicionesEPP",
                type: "datetime2",
                nullable: true);

            // Un documento sólo puede volver a tener un destinatario y una firma:
            // se conserva la primera de cada uno y el resto se pierde.
            migrationBuilder.Sql(@"
                UPDATE r
                SET r.[IdEmpleadoRecibe] = ISNULL(d.[IdEmpleadoDestino], 0)
                FROM [RequisicionesEPP] r
                INNER JOIN (
                    SELECT [IdRequisicion], [IdEmpleadoDestino],
                           ROW_NUMBER() OVER (PARTITION BY [IdRequisicion] ORDER BY [IdRequisicionDetalle]) AS rn
                    FROM [RequisicionesEPPDetalle]
                ) d ON d.[IdRequisicion] = r.[IdRequisicion] AND d.rn = 1;
            ");

            migrationBuilder.Sql(@"
                UPDATE r
                SET r.[FirmaDigital]     = e.[FirmaDigital],
                    r.[FotoEvidencia]    = e.[FotoEvidencia],
                    r.[Observaciones]    = e.[Observaciones],
                    r.[FechaEntrega]     = e.[FechaEntrega],
                    r.[IdUsuarioEntrega] = e.[IdUsuarioEntrega]
                FROM [RequisicionesEPP] r
                INNER JOIN (
                    SELECT [IdRequisicion], [FirmaDigital], [FotoEvidencia], [Observaciones],
                           [FechaEntrega], [IdUsuarioEntrega],
                           ROW_NUMBER() OVER (PARTITION BY [IdRequisicion] ORDER BY [IdRequisicionEntrega]) AS rn
                    FROM [RequisicionesEntregas]
                ) e ON e.[IdRequisicion] = r.[IdRequisicion] AND e.rn = 1;
            ");

            migrationBuilder.DropTable(name: "RequisicionesEntregas");

            migrationBuilder.DropIndex(
                name: "IX_RequisicionesEPPDetalle_EstadoRenglon",
                table: "RequisicionesEPPDetalle");

            migrationBuilder.DropIndex(
                name: "IX_RequisicionesEPPDetalle_IdConceptoPartida",
                table: "RequisicionesEPPDetalle");

            migrationBuilder.DropIndex(
                name: "IX_RequisicionesEPPDetalle_IdEmpleadoDestino",
                table: "RequisicionesEPPDetalle");

            migrationBuilder.DropIndex(
                name: "IX_RequisicionesEPPDetalle_IdProyectoDestino",
                table: "RequisicionesEPPDetalle");

            migrationBuilder.DropColumn(name: "EstadoRenglon", table: "RequisicionesEPPDetalle");
            migrationBuilder.DropColumn(name: "IdConceptoPartida", table: "RequisicionesEPPDetalle");
            migrationBuilder.DropColumn(name: "IdEmpleadoDestino", table: "RequisicionesEPPDetalle");
            migrationBuilder.DropColumn(name: "IdProyectoDestino", table: "RequisicionesEPPDetalle");
            migrationBuilder.DropColumn(name: "MotivoRechazo", table: "RequisicionesEPPDetalle");
            migrationBuilder.DropColumn(name: "FechaRequerida", table: "RequisicionesEPP");

            migrationBuilder.CreateIndex(
                name: "IX_RequisicionesEPP_IdEmpleadoRecibe",
                table: "RequisicionesEPP",
                column: "IdEmpleadoRecibe");

            migrationBuilder.CreateIndex(
                name: "IX_RequisicionesEPP_IdUsuarioEntrega",
                table: "RequisicionesEPP",
                column: "IdUsuarioEntrega");

            migrationBuilder.AddForeignKey(
                name: "FK_RequisicionesEPP_AspNetUsers_IdUsuarioEntrega",
                table: "RequisicionesEPP",
                column: "IdUsuarioEntrega",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RequisicionesEPP_Empleados_IdEmpleadoRecibe",
                table: "RequisicionesEPP",
                column: "IdEmpleadoRecibe",
                principalTable: "Empleados",
                principalColumn: "IdEmpleado",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
