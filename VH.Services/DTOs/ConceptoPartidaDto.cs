using System.ComponentModel.DataAnnotations;

namespace VH.Services.DTOs
{
    // DTO para respuestas (lectura)
    public class ConceptoPartidaResponseDto
    {
        public int IdPartida { get; set; }
        public int IdProyecto { get; set; }
        public string Descripcion { get; set; } = string.Empty;

        // Datos de UnidadMedida
        public int IdUnidadMedida { get; set; }
        public string NombreUnidadMedida { get; set; } = string.Empty;
        public string AbreviaturaUnidadMedida { get; set; } = string.Empty;

        public decimal CantidadEstimada { get; set; }

        /// <summary>Costo de material presupuestado por unidad de obra.</summary>
        public decimal? PrecioUnitarioEstimado { get; set; }

        /// <summary>Lo presupuestado. Nulo mientras la partida no tenga precio.</summary>
        public decimal? CostoTotalEstimado =>
            PrecioUnitarioEstimado.HasValue
                ? PrecioUnitarioEstimado.Value * CantidadEstimada
                : null;

        /// <summary>
        /// Lo que de verdad se ha gastado contra esta partida: la suma de cada
        /// salida cargada a ella, al costo del lote del que salió. No se guarda
        /// en ninguna columna; se calcula al consultar, así que no puede quedar
        /// desfasado de las salidas que lo componen.
        /// </summary>
        public decimal CostoConsumido { get; set; }

        /// <summary>Cuántas salidas de almacén componen ese consumo.</summary>
        public int SalidasConsumidas { get; set; }

        // ===== COMPARACIÓN =====

        /// <summary>
        /// Lo que queda. Negativo significa sobregiro. Nulo cuando no hay
        /// presupuesto capturado: sin estimado no hay diferencia que calcular, y
        /// un cero ahí se leería como "gastado todo".
        /// </summary>
        public decimal? Diferencia =>
            CostoTotalEstimado.HasValue ? CostoTotalEstimado.Value - CostoConsumido : null;

        /// <summary>Qué porcentaje del presupuesto se lleva consumido.</summary>
        public decimal? PorcentajeConsumido =>
            CostoTotalEstimado.HasValue && CostoTotalEstimado.Value > 0
                ? Math.Round(CostoConsumido / CostoTotalEstimado.Value * 100m, 1)
                : null;

        public bool TienePresupuesto => CostoTotalEstimado.HasValue;

        /// <summary>Se gastó más de lo presupuestado.</summary>
        public bool EstaSobregirada => Diferencia.HasValue && Diferencia.Value < 0;

        /// <summary>
        /// Se acerca al tope. Es un aviso, no un problema: sirve para mirar la
        /// partida antes de que se pase, no después.
        /// </summary>
        public bool EstaEnRiesgo =>
            !EstaSobregirada && PorcentajeConsumido.HasValue && PorcentajeConsumido.Value >= 80m;

        public string ClaseSemaforo => !TienePresupuesto ? "secondary"
            : EstaSobregirada ? "danger"
            : EstaEnRiesgo ? "warning"
            : "success";
    }

    // DTO para creación (escritura)
    public record ConceptoPartidaRequestDto(
        [Required(ErrorMessage = "La descripción es obligatoria")]
        [MaxLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
        string Descripcion,

        [Required(ErrorMessage = "La unidad de medida es obligatoria")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una unidad de medida válida")]
        int IdUnidadMedida,

        [Required(ErrorMessage = "La cantidad estimada es obligatoria")]
        [Range(0.01, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        decimal CantidadEstimada,

        /// <summary>Opcional: una partida sin presupuestar sigue siendo válida.</summary>
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio unitario debe ser mayor a 0")]
        decimal? PrecioUnitarioEstimado = null
    );

    /// <summary>
    /// Resumen de costos de una obra: lo contratado, lo repartido en partidas y
    /// lo gastado. Son tres números distintos y el sistema los tenía confundidos
    /// en una sola columna.
    /// </summary>
    public class ResumenCostosProyectoDto
    {
        public int IdProyecto { get; set; }
        public string NombreProyecto { get; set; } = string.Empty;

        /// <summary>Lo contratado para la obra. Se captura al darla de alta.</summary>
        public decimal PresupuestoObra { get; set; }

        /// <summary>Suma de lo presupuestado en las partidas que sí tienen precio.</summary>
        public decimal PresupuestadoEnPartidas { get; set; }

        /// <summary>Partidas todavía sin precio: su presupuesto no está en el total.</summary>
        public int PartidasSinPresupuesto { get; set; }

        public decimal ConsumidoEnPartidas { get; set; }

        /// <summary>
        /// Consumo cargado a la obra pero no a una partida concreta. Cuenta para
        /// el gasto de la obra y no aparece en el avance de ninguna partida.
        /// </summary>
        public decimal ConsumidoSinPartida { get; set; }

        public decimal ConsumidoTotal => ConsumidoEnPartidas + ConsumidoSinPartida;

        public int PartidasSobregiradas { get; set; }

        /// <summary>
        /// Lo repartido en partidas contra lo contratado. Pasarse significa que
        /// el presupuesto por concepto ya no cabe en la obra.
        /// </summary>
        public decimal? DisponibleDelPresupuesto =>
            PresupuestoObra > 0 ? PresupuestoObra - PresupuestadoEnPartidas : null;
    }
}
