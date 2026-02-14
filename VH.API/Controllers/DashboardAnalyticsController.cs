using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VH.Services.Interfaces;

namespace VH.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DashboardAnalyticsController : ControllerBase
    {
        private readonly IDashboardAnalyticsService _dashboardService;

        public DashboardAnalyticsController(IDashboardAnalyticsService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("kpis")]
        public async Task<IActionResult> GetKPIs([FromQuery] int? idProyecto = null)
        {
            var kpis = await _dashboardService.GetKPIsEjecutivosAsync(idProyecto);
            return Ok(kpis);
        }

        [HttpGet("ranking/{anio}/{mes}")]
        public async Task<IActionResult> GetRanking(int anio, int mes, [FromQuery] int? idProyecto = null, [FromQuery] int? idPuesto = null)
        {
            var ranking = await _dashboardService.GetRankingConsumidoresAsync(anio, mes, idProyecto, idPuesto);
            return Ok(ranking);
        }

        [HttpGet("consumo-proyectos/{anio}/{mes}")]
        public async Task<IActionResult> GetConsumoProyectos(int anio, int mes)
        {
            var consumo = await _dashboardService.GetConsumoProyectosAsync(anio, mes);
            return Ok(consumo);
        }

        [HttpGet("consumo-proyecto/{idProyecto}/{anio}/{mes}")]
        public async Task<IActionResult> GetConsumoProyectoDetalle(int idProyecto, int anio, int mes)
        {
            var consumo = await _dashboardService.GetConsumoProyectoDetalleAsync(idProyecto, anio, mes);
            if (consumo == null) return NotFound();
            return Ok(consumo);
        }

        [HttpGet("perfil-empleado/{idEmpleado}")]
        public async Task<IActionResult> GetPerfilEmpleado(int idEmpleado)
        {
            var perfil = await _dashboardService.GetPerfilEmpleadoAsync(idEmpleado);
            if (perfil == null) return NotFound();
            return Ok(perfil);
        }

        [HttpGet("historial-empleado/{idEmpleado}")]
        public async Task<IActionResult> GetHistorialEmpleado(int idEmpleado, [FromQuery] int meses = 12)
        {
            var historial = await _dashboardService.GetHistorialEmpleadoAsync(idEmpleado, meses);
            return Ok(historial);
        }

        [HttpGet("materiales-frecuentes/{idEmpleado}")]
        public async Task<IActionResult> GetMaterialesFrecuentes(int idEmpleado, [FromQuery] int top = 10)
        {
            var materiales = await _dashboardService.GetMaterialesFrecuentesEmpleadoAsync(idEmpleado, top);
            return Ok(materiales);
        }

        [HttpGet("tendencia-consumo")]
        public async Task<IActionResult> GetTendenciaConsumo([FromQuery] int meses = 12, [FromQuery] int? idProyecto = null)
        {
            var tendencia = await _dashboardService.GetTendenciaConsumoAsync(meses, idProyecto);
            return Ok(tendencia);
        }

        [HttpGet("tendencia-alertas")]
        public async Task<IActionResult> GetTendenciaAlertas([FromQuery] int meses = 12, [FromQuery] int? idProyecto = null)
        {
            var tendencia = await _dashboardService.GetTendenciaAlertasAsync(meses, idProyecto);
            return Ok(tendencia);
        }

        [HttpGet("heatmap/{idProyecto}/{anio}/{mes}")]
        public async Task<IActionResult> GetHeatmap(int idProyecto, int anio, int mes)
        {
            var heatmap = await _dashboardService.GetHeatmapFrecuenciaAsync(idProyecto, anio, mes);
            return Ok(heatmap);
        }

        [HttpGet("comparar-empleado/{idEmpleado}/{anio}/{mes}")]
        public async Task<IActionResult> CompararEmpleadoConPuesto(int idEmpleado, int anio, int mes)
        {
            var (promedio, desviacion, porcentaje) = await _dashboardService.CompararEmpleadoConPuestoAsync(idEmpleado, anio, mes);
            return Ok(new { promedio, desviacion, porcentajeVsPromedio = porcentaje });
        }

        [HttpGet("proyectos-desviacion")]
        public async Task<IActionResult> GetProyectosConDesviacion([FromQuery] decimal umbral = 10)
        {
            var proyectos = await _dashboardService.GetProyectosConDesviacionAsync(umbral);
            return Ok(proyectos);
        }

        [HttpGet("reporte-mensual/{anio}/{mes}")]
        public async Task<IActionResult> GetReporteMensual(int anio, int mes, [FromQuery] int? idProyecto = null)
        {
            var reporte = await _dashboardService.GenerarReporteMensualAsync(anio, mes, idProyecto);
            return Ok(reporte);
        }

        [HttpPost("recalcular-estadisticas/{anio}/{mes}")]
        public async Task<IActionResult> RecalcularEstadisticas(int anio, int mes)
        {
            await _dashboardService.RecalcularTodasEstadisticasAsync(anio, mes);
            return Ok(new { mensaje = "Estadísticas recalculadas correctamente" });
        }

        [HttpPost("recalcular-riesgos")]
        public async Task<IActionResult> RecalcularPuntuacionesRiesgo()
        {
            await _dashboardService.RecalcularTodasPuntuacionesRiesgoAsync();
            return Ok(new { mensaje = "Puntuaciones de riesgo recalculadas correctamente" });
        }

        [HttpGet("filtros/proyectos")]
        public async Task<IActionResult> GetProyectosParaFiltro()
        {
            var proyectos = await _dashboardService.GetProyectosParaFiltroAsync();
            return Ok(proyectos.Select(p => new { id = p.Id, nombre = p.Nombre }));
        }

        [HttpGet("filtros/puestos")]
        public async Task<IActionResult> GetPuestosParaFiltro()
        {
            var puestos = await _dashboardService.GetPuestosParaFiltroAsync();
            return Ok(puestos.Select(p => new { id = p.Id, nombre = p.Nombre }));
        }

        [HttpGet("filtros/materiales")]
        public async Task<IActionResult> GetMaterialesParaFiltro()
        {
            var materiales = await _dashboardService.GetMaterialesParaFiltroAsync();
            return Ok(materiales.Select(m => new { id = m.Id, nombre = m.Nombre }));
        }

        [HttpGet("filtros/periodos")]
        public async Task<IActionResult> GetPeriodosDisponibles()
        {
            var periodos = await _dashboardService.GetPeriodosDisponiblesAsync();
            return Ok(periodos.Select(p => new { anio = p.Anio, mes = p.Mes }));
        }
    }
}
