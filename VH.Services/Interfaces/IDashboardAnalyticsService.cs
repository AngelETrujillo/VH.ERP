using VH.Services.DTOs.Analytics;

namespace VH.Services.Interfaces
{
    /// <summary>
    /// Servicio para cálculo de métricas, rankings y visualizaciones del dashboard
    /// </summary>
    public interface IDashboardAnalyticsService
    {
        // ===== KPIs EJECUTIVOS =====

        /// <summary>
        /// Obtiene los KPIs principales para el dashboard ejecutivo
        /// </summary>
        Task<KPIsEjecutivosDto> GetKPIsEjecutivosAsync(int? idProyecto = null);

        // ===== RANKINGS =====

        /// <summary>
        /// Obtiene el ranking de consumidores (Top 10 + empates y Bottom 10 + empates)
        /// </summary>
        /// <param name="anio">Año del período</param>
        /// <param name="mes">Mes del período</param>
        /// <param name="idProyecto">Filtrar por proyecto (opcional)</param>
        /// <param name="idPuesto">Filtrar por puesto (opcional)</param>
        Task<RankingConsumoResponseDto> GetRankingConsumidoresAsync(int anio, int mes, int? idProyecto = null, int? idPuesto = null);

        /// <summary>
        /// Recalcula el ranking para el período actual
        /// </summary>
        Task RecalcularRankingAsync(int anio, int mes);

        // ===== CONSUMO POR PROYECTO =====

        /// <summary>
        /// Obtiene el consumo agregado por proyecto para el período
        /// </summary>
        Task<IEnumerable<ConsumoProyectoDto>> GetConsumoProyectosAsync(int anio, int mes);

        /// <summary>
        /// Obtiene detalle de consumo de un proyecto específico
        /// </summary>
        Task<ConsumoProyectoDto?> GetConsumoProyectoDetalleAsync(int idProyecto, int anio, int mes);

        // ===== PERFIL DE EMPLEADO =====

        /// <summary>
        /// Obtiene el perfil completo de consumo de un empleado
        /// </summary>
        Task<PerfilConsumoEmpleadoDto?> GetPerfilEmpleadoAsync(int idEmpleado);

        /// <summary>
        /// Obtiene el historial mensual de consumo de un empleado
        /// </summary>
        Task<IEnumerable<ConsumoMensualDto>> GetHistorialEmpleadoAsync(int idEmpleado, int meses = 12);

        /// <summary>
        /// Obtiene los materiales más solicitados por un empleado
        /// </summary>
        Task<IEnumerable<MaterialFrecuenteDto>> GetMaterialesFrecuentesEmpleadoAsync(int idEmpleado, int top = 10);

        // ===== TENDENCIAS =====

        /// <summary>
        /// Obtiene la tendencia de consumo para gráfico de línea
        /// </summary>
        Task<TendenciaConsumoDto> GetTendenciaConsumoAsync(int meses = 12, int? idProyecto = null);

        /// <summary>
        /// Obtiene tendencia de alertas generadas
        /// </summary>
        Task<TendenciaConsumoDto> GetTendenciaAlertasAsync(int meses = 12, int? idProyecto = null);

        // ===== HEATMAP =====

        /// <summary>
        /// Genera datos para heatmap de frecuencia empleado vs material
        /// </summary>
        Task<HeatmapFrecuenciaDto> GetHeatmapFrecuenciaAsync(int idProyecto, int anio, int mes);

        // ===== ESTADÍSTICAS =====

        /// <summary>
        /// Recalcula estadísticas mensuales de un empleado
        /// </summary>
        Task RecalcularEstadisticasEmpleadoAsync(int idEmpleado, int anio, int mes);

        /// <summary>
        /// Recalcula estadísticas mensuales de un proyecto
        /// </summary>
        Task RecalcularEstadisticasProyectoAsync(int idProyecto, int anio, int mes);

        /// <summary>
        /// Recalcula todas las estadísticas del período
        /// </summary>
        Task RecalcularTodasEstadisticasAsync(int anio, int mes);

        /// <summary>
        /// Calcula y actualiza la puntuación de riesgo de un empleado
        /// </summary>
        Task<decimal> CalcularPuntuacionRiesgoAsync(int idEmpleado);

        /// <summary>
        /// Recalcula puntuación de riesgo de todos los empleados activos
        /// </summary>
        Task RecalcularTodasPuntuacionesRiesgoAsync();

        // ===== COMPARATIVAS =====

        /// <summary>
        /// Obtiene el promedio de consumo por puesto para comparativas
        /// </summary>
        Task<decimal> GetPromedioConsumoPuestoAsync(int idPuesto, int anio, int mes);

        /// <summary>
        /// Obtiene la desviación estándar de consumo por puesto
        /// </summary>
        Task<decimal> GetDesviacionEstandarPuestoAsync(int idPuesto, int anio, int mes);

        /// <summary>
        /// Compara un empleado contra el promedio de su puesto
        /// </summary>
        Task<(decimal promedio, decimal desviacion, decimal porcentajeVsPromedio)> CompararEmpleadoConPuestoAsync(int idEmpleado, int anio, int mes);

        // ===== REPORTES =====

        /// <summary>
        /// Genera datos para reporte mensual de consumo
        /// </summary>
        Task<object> GenerarReporteMensualAsync(int anio, int mes, int? idProyecto = null);

        /// <summary>
        /// Obtiene resumen de desviaciones presupuestales
        /// </summary>
        Task<IEnumerable<ConsumoProyectoDto>> GetProyectosConDesviacionAsync(decimal umbralPorcentaje = 10);

        // ===== FILTROS DINÁMICOS =====

        /// <summary>
        /// Obtiene lista de proyectos para filtros del dashboard
        /// </summary>
        Task<IEnumerable<(int Id, string Nombre)>> GetProyectosParaFiltroAsync();

        /// <summary>
        /// Obtiene lista de puestos para filtros del dashboard
        /// </summary>
        Task<IEnumerable<(int Id, string Nombre)>> GetPuestosParaFiltroAsync();

        /// <summary>
        /// Obtiene lista de materiales para filtros del dashboard
        /// </summary>
        Task<IEnumerable<(int Id, string Nombre)>> GetMaterialesParaFiltroAsync();

        /// <summary>
        /// Obtiene períodos disponibles (años y meses con datos)
        /// </summary>
        Task<IEnumerable<(int Anio, int Mes)>> GetPeriodosDisponiblesAsync();
    }
}