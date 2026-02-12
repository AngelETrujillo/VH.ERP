using VH.Services.DTOs.Analytics;
using VH.Services.Entities;
using VH.Services.Interfaces;

namespace VH.Services.Services
{
    /// <summary>
    /// Implementación del servicio de analytics para dashboard
    /// </summary>
    public class DashboardAnalyticsService : IDashboardAnalyticsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAlertaConsumoService _alertaService;

        public DashboardAnalyticsService(IUnitOfWork unitOfWork, IAlertaConsumoService alertaService)
        {
            _unitOfWork = unitOfWork;
            _alertaService = alertaService;
        }

        #region KPIs Ejecutivos

        public async Task<KPIsEjecutivosDto> GetKPIsEjecutivosAsync(int? idProyecto = null)
        {
            var hoy = DateTime.Today;
            var inicioMesActual = new DateTime(hoy.Year, hoy.Month, 1);
            var inicioMesAnterior = inicioMesActual.AddMonths(-1);
            var inicioAnio = new DateTime(hoy.Year, 1, 1);

            // Obtener entregas
            var entregas = await _unitOfWork.EntregasEPP.FindAsync(
                e => e.FechaEntrega >= inicioAnio &&
                     (!idProyecto.HasValue || e.Empleado!.IdProyecto == idProyecto),
                "Compra.Material,Empleado");

            var entregasList = entregas.ToList();

            // Consumo mes actual
            var entregasMesActual = entregasList.Where(e => e.FechaEntrega >= inicioMesActual);
            var consumoMesActual = entregasMesActual.Sum(e => e.CantidadEntregada * (e.Compra?.PrecioUnitario ?? 0));

            // Consumo mes anterior
            var entregasMesAnterior = entregasList.Where(e => e.FechaEntrega >= inicioMesAnterior && e.FechaEntrega < inicioMesActual);
            var consumoMesAnterior = entregasMesAnterior.Sum(e => e.CantidadEntregada * (e.Compra?.PrecioUnitario ?? 0));

            // Variación
            var variacion = consumoMesAnterior > 0
                ? ((consumoMesActual - consumoMesAnterior) / consumoMesAnterior) * 100
                : 0;

            // Consumo acumulado año
            var consumoAcumulado = entregasList.Sum(e => e.CantidadEntregada * (e.Compra?.PrecioUnitario ?? 0));

            // Alertas
            var resumenAlertas = await _alertaService.GetResumenAlertasAsync(idProyecto);

            // Empleados con alertas
            var alertasPendientes = await _unitOfWork.AlertasConsumo.FindAsync(
                a => a.EstadoAlerta == EstadoAlerta.Pendiente &&
                     (!idProyecto.HasValue || a.IdProyecto == idProyecto));
            var empleadosConAlertas = alertasPendientes.Select(a => a.IdEmpleado).Distinct().Count();

            // Total empleados activos
            var empleados = await _unitOfWork.Empleados.FindAsync(
                e => e.Activo && (!idProyecto.HasValue || e.IdProyecto == idProyecto));
            var totalEmpleados = empleados.Count();

            // Proyectos sobre presupuesto
            var proyectos = await _unitOfWork.Proyectos.FindAsync(p => p.Activo);
            var proyectosSobre = 0;
            foreach (var proyecto in proyectos.Where(p => p.PresupuestoEPPMensual.HasValue && p.PresupuestoEPPMensual > 0))
            {
                var consumoProyecto = entregasMesActual
                    .Where(e => e.Empleado?.IdProyecto == proyecto.IdProyecto)
                    .Sum(e => e.CantidadEntregada * (e.Compra?.PrecioUnitario ?? 0));

                if (consumoProyecto > proyecto.PresupuestoEPPMensual)
                    proyectosSobre++;
            }

            return new KPIsEjecutivosDto
            {
                ConsumoMesActual = consumoMesActual,
                ConsumoMesAnterior = consumoMesAnterior,
                VariacionConsumoMensual = variacion,
                ConsumoAcumuladoAnio = consumoAcumulado,
                AlertasCriticas = resumenAlertas.TotalCriticas,
                AlertasAltas = resumenAlertas.TotalAltas,
                AlertasPendientes = resumenAlertas.TotalPendientes,
                CostoPerdidaPotencial = resumenAlertas.CostoPotencialTotal,
                EmpleadosConAlertas = empleadosConAlertas,
                TotalEmpleadosActivos = totalEmpleados,
                PorcentajeEmpleadosConAlertas = totalEmpleados > 0 ? (empleadosConAlertas / (decimal)totalEmpleados) * 100 : 0,
                ProyectosSobrePresupuesto = proyectosSobre,
                TotalProyectosActivos = proyectos.Count(p => !idProyecto.HasValue || p.IdProyecto == idProyecto)
            };
        }

        #endregion

        #region Rankings

        public async Task<RankingConsumoResponseDto> GetRankingConsumidoresAsync(int anio, int mes, int? idProyecto = null, int? idPuesto = null)
        {
            var inicioMes = new DateTime(anio, mes, 1);
            var finMes = inicioMes.AddMonths(1);

            // Obtener estadísticas del período
            var estadisticas = await _unitOfWork.EstadisticasEmpleadoMensual.FindAsync(
                e => e.Anio == anio && e.Mes == mes &&
                     (!idProyecto.HasValue || e.IdProyecto == idProyecto),
                "Empleado.Proyecto,Empleado.PuestoCatalogo");

            var estadisticasList = estadisticas.ToList();

            // Filtrar por puesto si se especificó
            if (idPuesto.HasValue)
            {
                estadisticasList = estadisticasList.Where(e => e.Empleado?.IdPuesto == idPuesto).ToList();
            }

            if (!estadisticasList.Any())
            {
                // Si no hay estadísticas precalculadas, calcular en tiempo real
                estadisticasList = await CalcularEstadisticasEnTiempoRealAsync(anio, mes, idProyecto, idPuesto);
            }

            // Calcular promedio general
            var promedioGeneral = estadisticasList.Any() ? estadisticasList.Average(e => e.CostoTotal) : 0;

            // Ordenar por costo total descendente para Top
            var ordenadoDesc = estadisticasList.OrderByDescending(e => e.CostoTotal).ToList();

            // Top 10 + empates
            var topConsumidores = ObtenerTopConEmpates(ordenadoDesc, 10, true, promedioGeneral);

            // Ordenar ascendente para Bottom
            var ordenadoAsc = estadisticasList.OrderBy(e => e.CostoTotal).ToList();

            // Bottom 10 + empates
            var bottomConsumidores = ObtenerTopConEmpates(ordenadoAsc, 10, false, promedioGeneral);

            return new RankingConsumoResponseDto
            {
                TopConsumidores = topConsumidores,
                BottomConsumidores = bottomConsumidores,
                TotalEmpleadosAnalizados = estadisticasList.Count,
                PromedioGeneral = promedioGeneral,
                FechaCalculo = DateTime.Now,
                Anio = anio,
                Mes = mes
            };
        }

        private List<RankingConsumidorDto> ObtenerTopConEmpates(
            List<EstadisticaEmpleadoMensual> ordenado,
            int top,
            bool esTop,
            decimal promedioGeneral)
        {
            var resultado = new List<RankingConsumidorDto>();
            if (!ordenado.Any()) return resultado;

            decimal? valorPosicion10 = null;
            var posicion = 0;

            foreach (var est in ordenado)
            {
                posicion++;

                // Si ya pasamos del top 10 y el valor es diferente al de posición 10, terminar
                if (posicion > top && valorPosicion10.HasValue && est.CostoTotal != valorPosicion10)
                    break;

                // Guardar valor de posición 10 para detectar empates
                if (posicion == top)
                    valorPosicion10 = est.CostoTotal;

                var desviacion = promedioGeneral > 0
                    ? ((est.CostoTotal - promedioGeneral) / promedioGeneral) * 100
                    : 0;

                resultado.Add(new RankingConsumidorDto
                {
                    Posicion = posicion > top ? top : posicion, // Los empates comparten posición 10
                    IdEmpleado = est.IdEmpleado,
                    NumeroNomina = est.Empleado?.NumeroNomina ?? "",
                    NombreCompleto = est.Empleado != null
                        ? $"{est.Empleado.Nombre} {est.Empleado.ApellidoPaterno} {est.Empleado.ApellidoMaterno}".Trim()
                        : "",
                    Puesto = est.Empleado?.PuestoCatalogo?.Nombre ?? est.Empleado?.Puesto ?? "",
                    NombreProyecto = est.Proyecto?.Nombre ?? est.Empleado?.Proyecto?.Nombre ?? "",
                    TotalEntregas = est.TotalEntregas,
                    TotalUnidades = est.TotalUnidades,
                    CostoTotal = est.CostoTotal,
                    PromedioGrupoPuesto = promedioGeneral, // TODO: Calcular por puesto específico
                    DesviacionPorcentaje = desviacion,
                    PuntuacionRiesgo = est.PuntuacionRiesgo,
                    AlertasActivas = est.AlertasGeneradas,
                    NivelRiesgo = GetNivelRiesgo(est.PuntuacionRiesgo),
                    ColorIndicador = GetColorRiesgo(est.PuntuacionRiesgo),
                    EsEmpate = posicion > top
                });
            }

            return resultado;
        }

        private async Task<List<EstadisticaEmpleadoMensual>> CalcularEstadisticasEnTiempoRealAsync(
            int anio, int mes, int? idProyecto, int? idPuesto)
        {
            var inicioMes = new DateTime(anio, mes, 1);
            var finMes = inicioMes.AddMonths(1);

            var entregas = await _unitOfWork.EntregasEPP.FindAsync(
                e => e.FechaEntrega >= inicioMes && e.FechaEntrega < finMes &&
                     (!idProyecto.HasValue || e.Empleado!.IdProyecto == idProyecto),
                "Compra,Empleado.Proyecto,Empleado.PuestoCatalogo");

            var agrupado = entregas
                .GroupBy(e => e.IdEmpleado)
                .Select(g => new EstadisticaEmpleadoMensual
                {
                    IdEmpleado = g.Key,
                    IdProyecto = g.First().Empleado?.IdProyecto ?? 0,
                    Anio = anio,
                    Mes = mes,
                    TotalEntregas = g.Count(),
                    TotalUnidades = g.Sum(e => e.CantidadEntregada),
                    CostoTotal = g.Sum(e => e.CantidadEntregada * (e.Compra?.PrecioUnitario ?? 0)),
                    MaterialesDistintos = g.Select(e => e.Compra?.IdMaterial).Distinct().Count(),
                    Empleado = g.First().Empleado,
                    Proyecto = g.First().Empleado?.Proyecto
                })
                .ToList();

            // Filtrar por puesto si se especificó
            if (idPuesto.HasValue)
            {
                agrupado = agrupado.Where(e => e.Empleado?.IdPuesto == idPuesto).ToList();
            }

            return agrupado;
        }

        public async Task RecalcularRankingAsync(int anio, int mes)
        {
            await RecalcularTodasEstadisticasAsync(anio, mes);
        }

        #endregion

        #region Consumo por Proyecto

        public async Task<IEnumerable<ConsumoProyectoDto>> GetConsumoProyectosAsync(int anio, int mes)
        {
            var estadisticas = await _unitOfWork.EstadisticasProyectoMensual.FindAsync(
                e => e.Anio == anio && e.Mes == mes,
                "Proyecto");

            return estadisticas.Select(e => new ConsumoProyectoDto
            {
                IdProyecto = e.IdProyecto,
                NombreProyecto = e.Proyecto?.Nombre ?? "",
                TipoObra = e.Proyecto?.TipoObra ?? "",
                CostoTotal = e.CostoTotal,
                TotalEntregas = e.TotalEntregas,
                TotalEmpleados = e.TotalEmpleados,
                CostoPromedioPorEmpleado = e.CostoPromedioPorEmpleado,
                PresupuestoAsignado = e.PresupuestoAsignado,
                DesviacionPresupuesto = e.DesviacionPresupuesto,
                SobrePresupuesto = e.DesviacionPresupuesto > 0,
                AlertasCriticas = e.AlertasCriticas,
                TotalAlertas = e.TotalAlertas,
                ColorBarra = e.DesviacionPresupuesto > 20 ? "danger" : e.DesviacionPresupuesto > 0 ? "warning" : "success"
            });
        }

        public async Task<ConsumoProyectoDto?> GetConsumoProyectoDetalleAsync(int idProyecto, int anio, int mes)
        {
            var estadisticas = await _unitOfWork.EstadisticasProyectoMensual.FindAsync(
                e => e.IdProyecto == idProyecto && e.Anio == anio && e.Mes == mes,
                "Proyecto");

            var est = estadisticas.FirstOrDefault();
            if (est == null) return null;

            return new ConsumoProyectoDto
            {
                IdProyecto = est.IdProyecto,
                NombreProyecto = est.Proyecto?.Nombre ?? "",
                TipoObra = est.Proyecto?.TipoObra ?? "",
                CostoTotal = est.CostoTotal,
                TotalEntregas = est.TotalEntregas,
                TotalEmpleados = est.TotalEmpleados,
                CostoPromedioPorEmpleado = est.CostoPromedioPorEmpleado,
                PresupuestoAsignado = est.PresupuestoAsignado,
                DesviacionPresupuesto = est.DesviacionPresupuesto,
                SobrePresupuesto = est.DesviacionPresupuesto > 0,
                AlertasCriticas = est.AlertasCriticas,
                TotalAlertas = est.TotalAlertas
            };
        }

        #endregion

        #region Perfil de Empleado

        public async Task<PerfilConsumoEmpleadoDto?> GetPerfilEmpleadoAsync(int idEmpleado)
        {
            var empleado = await _unitOfWork.Empleados.GetByIdAsync(idEmpleado, "Proyecto,PuestoCatalogo");
            if (empleado == null) return null;

            var historial = await GetHistorialEmpleadoAsync(idEmpleado, 12);
            var materialesFrecuentes = await GetMaterialesFrecuentesEmpleadoAsync(idEmpleado, 10);

            // Últimas entregas
            var entregas = await _unitOfWork.EntregasEPP.FindAsync(
                e => e.IdEmpleado == idEmpleado,
                "Compra.Material");
            var ultimasEntregas = entregas
                .OrderByDescending(e => e.FechaEntrega)
                .Take(10)
                .Select(e => new EntregaResumenDto
                {
                    IdEntrega = e.IdEntrega,
                    FechaEntrega = e.FechaEntrega,
                    NombreMaterial = e.Compra?.Material?.Nombre ?? "",
                    Cantidad = e.CantidadEntregada,
                    Costo = e.CantidadEntregada * (e.Compra?.PrecioUnitario ?? 0)
                })
                .ToList();

            // Alertas
            var alertas = await _unitOfWork.AlertasConsumo.FindAsync(
                a => a.IdEmpleado == idEmpleado,
                "Material");
            var alertasList = alertas.ToList();

            var alertasRecientes = alertasList
                .OrderByDescending(a => a.FechaGeneracion)
                .Take(10)
                .Select(a => new AlertaResumenDto
                {
                    IdAlerta = a.IdAlerta,
                    FechaGeneracion = a.FechaGeneracion,
                    TipoAlerta = a.TipoAlerta.ToString(),
                    Severidad = a.Severidad.ToString(),
                    Descripcion = a.Descripcion,
                    Estado = a.EstadoAlerta.ToString(),
                    ColorSeveridad = GetColorRiesgo((int)a.Severidad * 25)
                })
                .ToList();

            // Calcular totales
            var costoTotal = entregas.Sum(e => e.CantidadEntregada * (e.Compra?.PrecioUnitario ?? 0));
            var historialList = historial.ToList();
            var promedioMensual = historialList.Any() ? historialList.Average(h => h.CostoTotal) : 0;

            return new PerfilConsumoEmpleadoDto
            {
                IdEmpleado = empleado.IdEmpleado,
                NumeroNomina = empleado.NumeroNomina,
                NombreCompleto = $"{empleado.Nombre} {empleado.ApellidoPaterno} {empleado.ApellidoMaterno}".Trim(),
                Puesto = empleado.PuestoCatalogo?.Nombre ?? empleado.Puesto,
                NombreProyecto = empleado.Proyecto?.Nombre ?? "",
                FechaIngreso = empleado.FechaIngreso,
                DiasEnEmpresa = empleado.FechaIngreso.HasValue
                    ? (int)(DateTime.Now - empleado.FechaIngreso.Value).TotalDays
                    : 0,
                PuntuacionRiesgo = empleado.PuntuacionRiesgoActual,
                NivelRiesgo = GetNivelRiesgo(empleado.PuntuacionRiesgoActual),
                FechaUltimoCalculoRiesgo = empleado.FechaUltimoCalculoRiesgo,
                CostoTotalHistorico = costoTotal,
                TotalEntregasHistorico = entregas.Count(),
                PromedioMensual = promedioMensual,
                AlertasPendientes = alertasList.Count(a => a.EstadoAlerta == EstadoAlerta.Pendiente),
                AlertasConfirmadas = alertasList.Count(a => a.EstadoAlerta == EstadoAlerta.Confirmada),
                AlertasDescartadas = alertasList.Count(a => a.EstadoAlerta == EstadoAlerta.Descartada),
                HistorialMensual = historialList,
                MaterialesFrecuentes = materialesFrecuentes.ToList(),
                UltimasEntregas = ultimasEntregas,
                AlertasRecientes = alertasRecientes
            };
        }

        public async Task<IEnumerable<ConsumoMensualDto>> GetHistorialEmpleadoAsync(int idEmpleado, int meses = 12)
        {
            var fechaInicio = DateTime.Now.AddMonths(-meses);
            var estadisticas = await _unitOfWork.EstadisticasEmpleadoMensual.FindAsync(
                e => e.IdEmpleado == idEmpleado &&
                     (e.Anio > fechaInicio.Year || (e.Anio == fechaInicio.Year && e.Mes >= fechaInicio.Month)));

            return estadisticas
                .OrderBy(e => e.Anio).ThenBy(e => e.Mes)
                .Select(e => new ConsumoMensualDto
                {
                    Anio = e.Anio,
                    Mes = e.Mes,
                    Periodo = $"{GetNombreMes(e.Mes)} {e.Anio}",
                    TotalEntregas = e.TotalEntregas,
                    CostoTotal = e.CostoTotal,
                    AlertasGeneradas = e.AlertasGeneradas,
                    PuntuacionRiesgo = e.PuntuacionRiesgo
                });
        }

        public async Task<IEnumerable<MaterialFrecuenteDto>> GetMaterialesFrecuentesEmpleadoAsync(int idEmpleado, int top = 10)
        {
            var entregas = await _unitOfWork.EntregasEPP.FindAsync(
                e => e.IdEmpleado == idEmpleado,
                "Compra.Material");

            var agrupado = entregas
                .Where(e => e.Compra?.Material != null)
                .GroupBy(e => e.Compra!.IdMaterial)
                .Select(g => new MaterialFrecuenteDto
                {
                    IdMaterial = g.Key,
                    NombreMaterial = g.First().Compra?.Material?.Nombre ?? "",
                    VecesSolicitado = g.Count(),
                    CantidadTotal = g.Sum(e => e.CantidadEntregada),
                    CostoTotal = g.Sum(e => e.CantidadEntregada * (e.Compra?.PrecioUnitario ?? 0)),
                    VidaUtilEsperada = g.First().Compra?.Material?.VidaUtilDiasDefault
                })
                .OrderByDescending(m => m.VecesSolicitado)
                .Take(top);

            return agrupado;
        }

        #endregion

        #region Tendencias

        public async Task<TendenciaConsumoDto> GetTendenciaConsumoAsync(int meses = 12, int? idProyecto = null)
        {
            var fechaInicio = DateTime.Now.AddMonths(-meses);

            var estadisticas = idProyecto.HasValue
                ? await _unitOfWork.EstadisticasProyectoMensual.FindAsync(
                    e => e.IdProyecto == idProyecto &&
                         (e.Anio > fechaInicio.Year || (e.Anio == fechaInicio.Year && e.Mes >= fechaInicio.Month)),
                    "Proyecto")
                : await _unitOfWork.EstadisticasProyectoMensual.FindAsync(
                    e => e.Anio > fechaInicio.Year || (e.Anio == fechaInicio.Year && e.Mes >= fechaInicio.Month));

            var puntos = estadisticas
                .GroupBy(e => new { e.Anio, e.Mes })
                .Select(g => new PuntoTendenciaDto
                {
                    Anio = g.Key.Anio,
                    Mes = g.Key.Mes,
                    Etiqueta = $"{GetNombreMesCorto(g.Key.Mes)} {g.Key.Anio}",
                    Valor = g.Sum(e => e.CostoTotal),
                    PresupuestoReferencia = g.Sum(e => e.PresupuestoAsignado)
                })
                .OrderBy(p => p.Anio).ThenBy(p => p.Mes)
                .ToList();

            var promedio = puntos.Any() ? puntos.Average(p => p.Valor) : 0;

            // Calcular tendencia (regresión lineal simple)
            decimal tendencia = 0;
            if (puntos.Count > 1)
            {
                var primeros = puntos.Take(puntos.Count / 2).Average(p => p.Valor);
                var ultimos = puntos.Skip(puntos.Count / 2).Average(p => p.Valor);
                tendencia = primeros > 0 ? ((ultimos - primeros) / primeros) * 100 : 0;
            }

            return new TendenciaConsumoDto
            {
                Puntos = puntos,
                PromedioHistorico = promedio,
                TendenciaPorcentaje = tendencia,
                Direccion = tendencia > 5 ? "Ascendente" : tendencia < -5 ? "Descendente" : "Estable"
            };
        }

        public async Task<TendenciaConsumoDto> GetTendenciaAlertasAsync(int meses = 12, int? idProyecto = null)
        {
            var fechaInicio = DateTime.Now.AddMonths(-meses);

            var alertas = await _unitOfWork.AlertasConsumo.FindAsync(
                a => a.FechaGeneracion >= fechaInicio &&
                     (!idProyecto.HasValue || a.IdProyecto == idProyecto));

            var puntos = alertas
                .GroupBy(a => new { a.FechaGeneracion.Year, a.FechaGeneracion.Month })
                .Select(g => new PuntoTendenciaDto
                {
                    Anio = g.Key.Year,
                    Mes = g.Key.Month,
                    Etiqueta = $"{GetNombreMesCorto(g.Key.Month)} {g.Key.Year}",
                    Valor = g.Count()
                })
                .OrderBy(p => p.Anio).ThenBy(p => p.Mes)
                .ToList();

            var promedio = puntos.Any() ? puntos.Average(p => p.Valor) : 0;

            return new TendenciaConsumoDto
            {
                Puntos = puntos,
                PromedioHistorico = promedio
            };
        }

        #endregion

        #region Heatmap

        public async Task<HeatmapFrecuenciaDto> GetHeatmapFrecuenciaAsync(int idProyecto, int anio, int mes)
        {
            var inicioMes = new DateTime(anio, mes, 1);
            var finMes = inicioMes.AddMonths(1);

            var entregas = await _unitOfWork.EntregasEPP.FindAsync(
                e => e.Empleado!.IdProyecto == idProyecto &&
                     e.FechaEntrega >= inicioMes && e.FechaEntrega < finMes,
                "Empleado,Compra.Material");

            var materiales = entregas
                .Where(e => e.Compra?.Material != null)
                .Select(e => e.Compra!.Material!)
                .DistinctBy(m => m.IdMaterial)
                .OrderBy(m => m.Nombre)
                .ToList();

            var empleados = entregas
                .Where(e => e.Empleado != null)
                .Select(e => e.Empleado!)
                .DistinctBy(e => e.IdEmpleado)
                .OrderBy(e => e.ApellidoPaterno)
                .ToList();

            var maxValor = 0m;
            var filas = new List<FilaHeatmapDto>();

            foreach (var empleado in empleados)
            {
                var celdas = new List<CeldaHeatmapDto>();
                foreach (var material in materiales)
                {
                    var cantidad = entregas
                        .Where(e => e.IdEmpleado == empleado.IdEmpleado && e.Compra?.IdMaterial == material.IdMaterial)
                        .Sum(e => e.CantidadEntregada);

                    if (cantidad > maxValor) maxValor = cantidad;

                    celdas.Add(new CeldaHeatmapDto
                    {
                        IdMaterial = material.IdMaterial,
                        Valor = cantidad,
                        Tooltip = $"{empleado.Nombre}: {cantidad} {material.Nombre}"
                    });
                }

                filas.Add(new FilaHeatmapDto
                {
                    IdEmpleado = empleado.IdEmpleado,
                    NombreEmpleado = $"{empleado.Nombre} {empleado.ApellidoPaterno}",
                    Celdas = celdas
                });
            }

            // Asignar colores basados en intensidad
            foreach (var fila in filas)
            {
                foreach (var celda in fila.Celdas)
                {
                    celda.Color = GetColorHeatmap(celda.Valor, maxValor);
                }
            }

            return new HeatmapFrecuenciaDto
            {
                Filas = filas,
                ColumnasEtiquetas = materiales.Select(m => m.Nombre).ToList(),
                ValorMaximo = maxValor
            };
        }

        #endregion

        #region Estadísticas

        public async Task RecalcularEstadisticasEmpleadoAsync(int idEmpleado, int anio, int mes)
        {
            var inicioMes = new DateTime(anio, mes, 1);
            var finMes = inicioMes.AddMonths(1);

            var empleado = await _unitOfWork.Empleados.GetByIdAsync(idEmpleado);
            if (empleado == null) return;

            var entregas = await _unitOfWork.EntregasEPP.FindAsync(
                e => e.IdEmpleado == idEmpleado && e.FechaEntrega >= inicioMes && e.FechaEntrega < finMes,
                "Compra");

            var alertas = await _unitOfWork.AlertasConsumo.FindAsync(
                a => a.IdEmpleado == idEmpleado && a.FechaGeneracion >= inicioMes && a.FechaGeneracion < finMes);

            var existente = (await _unitOfWork.EstadisticasEmpleadoMensual.FindAsync(
                e => e.IdEmpleado == idEmpleado && e.Anio == anio && e.Mes == mes)).FirstOrDefault();

            var estadistica = existente ?? new EstadisticaEmpleadoMensual
            {
                IdEmpleado = idEmpleado,
                IdProyecto = empleado.IdProyecto,
                Anio = anio,
                Mes = mes
            };

            estadistica.TotalEntregas = entregas.Count();
            estadistica.TotalUnidades = entregas.Sum(e => e.CantidadEntregada);
            estadistica.CostoTotal = entregas.Sum(e => e.CantidadEntregada * (e.Compra?.PrecioUnitario ?? 0));
            estadistica.MaterialesDistintos = entregas.Select(e => e.Compra?.IdMaterial).Distinct().Count();
            estadistica.AlertasGeneradas = alertas.Count();
            estadistica.FechaActualizacion = DateTime.Now;

            if (existente == null)
                await _unitOfWork.EstadisticasEmpleadoMensual.AddAsync(estadistica);
            else
                _unitOfWork.EstadisticasEmpleadoMensual.Update(estadistica);

            await _unitOfWork.CompleteAsync();
        }

        public async Task RecalcularEstadisticasProyectoAsync(int idProyecto, int anio, int mes)
        {
            var inicioMes = new DateTime(anio, mes, 1);
            var finMes = inicioMes.AddMonths(1);

            var proyecto = await _unitOfWork.Proyectos.GetByIdAsync(idProyecto);
            if (proyecto == null) return;

            var empleados = await _unitOfWork.Empleados.FindAsync(e => e.IdProyecto == idProyecto && e.Activo);
            var entregas = await _unitOfWork.EntregasEPP.FindAsync(
                e => e.Empleado!.IdProyecto == idProyecto && e.FechaEntrega >= inicioMes && e.FechaEntrega < finMes,
                "Compra,Empleado");

            var alertas = await _unitOfWork.AlertasConsumo.FindAsync(
                a => a.IdProyecto == idProyecto && a.FechaGeneracion >= inicioMes && a.FechaGeneracion < finMes);

            var existente = (await _unitOfWork.EstadisticasProyectoMensual.FindAsync(
                e => e.IdProyecto == idProyecto && e.Anio == anio && e.Mes == mes)).FirstOrDefault();

            var estadistica = existente ?? new EstadisticaProyectoMensual
            {
                IdProyecto = idProyecto,
                Anio = anio,
                Mes = mes
            };

            var costoTotal = entregas.Sum(e => e.CantidadEntregada * (e.Compra?.PrecioUnitario ?? 0));
            var totalEmpleados = empleados.Count();

            estadistica.TotalEmpleados = totalEmpleados;
            estadistica.TotalEntregas = entregas.Count();
            estadistica.TotalUnidades = entregas.Sum(e => e.CantidadEntregada);
            estadistica.CostoTotal = costoTotal;
            estadistica.CostoPromedioPorEmpleado = totalEmpleados > 0 ? costoTotal / totalEmpleados : 0;
            estadistica.PresupuestoAsignado = proyecto.PresupuestoEPPMensual ?? 0;
            estadistica.DesviacionPresupuesto = estadistica.PresupuestoAsignado > 0
                ? ((costoTotal - estadistica.PresupuestoAsignado) / estadistica.PresupuestoAsignado) * 100
                : 0;
            estadistica.AlertasCriticas = alertas.Count(a => a.Severidad == SeveridadAlerta.Critica);
            estadistica.TotalAlertas = alertas.Count();
            estadistica.FechaActualizacion = DateTime.Now;

            if (existente == null)
                await _unitOfWork.EstadisticasProyectoMensual.AddAsync(estadistica);
            else
                _unitOfWork.EstadisticasProyectoMensual.Update(estadistica);

            await _unitOfWork.CompleteAsync();
        }

        public async Task RecalcularTodasEstadisticasAsync(int anio, int mes)
        {
            var empleados = await _unitOfWork.Empleados.FindAsync(e => e.Activo);
            foreach (var empleado in empleados)
            {
                await RecalcularEstadisticasEmpleadoAsync(empleado.IdEmpleado, anio, mes);
            }

            var proyectos = await _unitOfWork.Proyectos.FindAsync(p => p.Activo);
            foreach (var proyecto in proyectos)
            {
                await RecalcularEstadisticasProyectoAsync(proyecto.IdProyecto, anio, mes);
            }
        }

        public async Task<decimal> CalcularPuntuacionRiesgoAsync(int idEmpleado)
        {
            var empleado = await _unitOfWork.Empleados.GetByIdAsync(idEmpleado);
            if (empleado == null) return 0;

            // Factores de puntuación
            var alertasPendientes = await _unitOfWork.AlertasConsumo.FindAsync(
                a => a.IdEmpleado == idEmpleado && a.EstadoAlerta == EstadoAlerta.Pendiente);
            var alertasConfirmadas = await _unitOfWork.AlertasConsumo.FindAsync(
                a => a.IdEmpleado == idEmpleado && a.EstadoAlerta == EstadoAlerta.Confirmada);

            var puntuacion = 0m;

            // Alertas pendientes (máx 40 puntos)
            puntuacion += Math.Min(alertasPendientes.Count() * 10, 40);

            // Alertas confirmadas históricas (máx 30 puntos)
            puntuacion += Math.Min(alertasConfirmadas.Count() * 15, 30);

            // Severidad de alertas pendientes (máx 30 puntos)
            var severidadPuntos = alertasPendientes.Sum(a => a.Severidad switch
            {
                SeveridadAlerta.Critica => 10,
                SeveridadAlerta.Alta => 7,
                SeveridadAlerta.Media => 4,
                _ => 1
            });
            puntuacion += Math.Min(severidadPuntos, 30);

            puntuacion = Math.Min(puntuacion, 100);

            // Actualizar empleado
            empleado.PuntuacionRiesgoActual = puntuacion;
            empleado.FechaUltimoCalculoRiesgo = DateTime.Now;
            _unitOfWork.Empleados.Update(empleado);
            await _unitOfWork.CompleteAsync();

            return puntuacion;
        }

        public async Task RecalcularTodasPuntuacionesRiesgoAsync()
        {
            var empleados = await _unitOfWork.Empleados.FindAsync(e => e.Activo);
            foreach (var empleado in empleados)
            {
                await CalcularPuntuacionRiesgoAsync(empleado.IdEmpleado);
            }
        }

        #endregion

        #region Comparativas

        public async Task<decimal> GetPromedioConsumoPuestoAsync(int idPuesto, int anio, int mes)
        {
            var estadisticas = await _unitOfWork.EstadisticasEmpleadoMensual.FindAsync(
                e => e.Anio == anio && e.Mes == mes && e.Empleado!.IdPuesto == idPuesto,
                "Empleado");

            return estadisticas.Any() ? estadisticas.Average(e => e.CostoTotal) : 0;
        }

        public async Task<decimal> GetDesviacionEstandarPuestoAsync(int idPuesto, int anio, int mes)
        {
            var estadisticas = await _unitOfWork.EstadisticasEmpleadoMensual.FindAsync(
                e => e.Anio == anio && e.Mes == mes && e.Empleado!.IdPuesto == idPuesto,
                "Empleado");

            var lista = estadisticas.Select(e => (double)e.CostoTotal).ToList();
            if (lista.Count < 2) return 0;

            var promedio = lista.Average();
            var sumaCuadrados = lista.Sum(x => Math.Pow(x - promedio, 2));
            return (decimal)Math.Sqrt(sumaCuadrados / (lista.Count - 1));
        }

        public async Task<(decimal promedio, decimal desviacion, decimal porcentajeVsPromedio)> CompararEmpleadoConPuestoAsync(int idEmpleado, int anio, int mes)
        {
            var empleado = await _unitOfWork.Empleados.GetByIdAsync(idEmpleado);
            if (empleado?.IdPuesto == null) return (0, 0, 0);

            var promedio = await GetPromedioConsumoPuestoAsync(empleado.IdPuesto.Value, anio, mes);
            var desviacion = await GetDesviacionEstandarPuestoAsync(empleado.IdPuesto.Value, anio, mes);

            var estadistica = (await _unitOfWork.EstadisticasEmpleadoMensual.FindAsync(
                e => e.IdEmpleado == idEmpleado && e.Anio == anio && e.Mes == mes)).FirstOrDefault();

            var costoEmpleado = estadistica?.CostoTotal ?? 0;
            var porcentaje = promedio > 0 ? ((costoEmpleado - promedio) / promedio) * 100 : 0;

            return (promedio, desviacion, porcentaje);
        }

        #endregion

        #region Reportes y Filtros

        public async Task<object> GenerarReporteMensualAsync(int anio, int mes, int? idProyecto = null)
        {
            var kpis = await GetKPIsEjecutivosAsync(idProyecto);
            var ranking = await GetRankingConsumidoresAsync(anio, mes, idProyecto);
            var proyectos = await GetConsumoProyectosAsync(anio, mes);
            var tendencia = await GetTendenciaConsumoAsync(12, idProyecto);
            var alertas = await _alertaService.GetResumenAlertasAsync(idProyecto);

            return new
            {
                Periodo = $"{GetNombreMes(mes)} {anio}",
                KPIs = kpis,
                Ranking = ranking,
                Proyectos = proyectos,
                Tendencia = tendencia,
                Alertas = alertas,
                FechaGeneracion = DateTime.Now
            };
        }

        public async Task<IEnumerable<ConsumoProyectoDto>> GetProyectosConDesviacionAsync(decimal umbralPorcentaje = 10)
        {
            var hoy = DateTime.Today;
            var proyectos = await GetConsumoProyectosAsync(hoy.Year, hoy.Month);
            return proyectos.Where(p => Math.Abs(p.DesviacionPresupuesto) > umbralPorcentaje);
        }

        public async Task<IEnumerable<(int Id, string Nombre)>> GetProyectosParaFiltroAsync()
        {
            var proyectos = await _unitOfWork.Proyectos.FindAsync(p => p.Activo);
            return proyectos.Select(p => (p.IdProyecto, p.Nombre));
        }

        public async Task<IEnumerable<(int Id, string Nombre)>> GetPuestosParaFiltroAsync()
        {
            var puestos = await _unitOfWork.Puestos.FindAsync(p => p.Activo);
            return puestos.Select(p => (p.IdPuesto, p.Nombre));
        }

        public async Task<IEnumerable<(int Id, string Nombre)>> GetMaterialesParaFiltroAsync()
        {
            var materiales = await _unitOfWork.MaterialesEPP.FindAsync(m => m.Activo);
            return materiales.Select(m => (m.IdMaterial, m.Nombre));
        }

        public async Task<IEnumerable<(int Anio, int Mes)>> GetPeriodosDisponiblesAsync()
        {
            var estadisticas = await _unitOfWork.EstadisticasProyectoMensual.GetAllAsync();
            return estadisticas
                .Select(e => (e.Anio, e.Mes))
                .Distinct()
                .OrderByDescending(p => p.Anio)
                .ThenByDescending(p => p.Mes);
        }

        #endregion

        #region Helpers

        private static string GetNivelRiesgo(decimal puntuacion) => puntuacion switch
        {
            >= 80 => "Crítico",
            >= 60 => "Alto",
            >= 40 => "Medio",
            >= 20 => "Bajo",
            _ => "Normal"
        };

        private static string GetColorRiesgo(decimal puntuacion) => puntuacion switch
        {
            >= 80 => "danger",
            >= 60 => "warning",
            >= 40 => "info",
            _ => "success"
        };

        private static string GetColorHeatmap(decimal valor, decimal max)
        {
            if (max == 0) return "#f0f0f0";
            var intensidad = valor / max;
            return intensidad switch
            {
                >= 0.8m => "#dc3545",
                >= 0.6m => "#fd7e14",
                >= 0.4m => "#ffc107",
                >= 0.2m => "#20c997",
                > 0 => "#198754",
                _ => "#f0f0f0"
            };
        }

        private static string GetNombreMes(int mes) => mes switch
        {
            1 => "Enero",
            2 => "Febrero",
            3 => "Marzo",
            4 => "Abril",
            5 => "Mayo",
            6 => "Junio",
            7 => "Julio",
            8 => "Agosto",
            9 => "Septiembre",
            10 => "Octubre",
            11 => "Noviembre",
            12 => "Diciembre",
            _ => ""
        };

        private static string GetNombreMesCorto(int mes) => mes switch
        {
            1 => "Ene",
            2 => "Feb",
            3 => "Mar",
            4 => "Abr",
            5 => "May",
            6 => "Jun",
            7 => "Jul",
            8 => "Ago",
            9 => "Sep",
            10 => "Oct",
            11 => "Nov",
            12 => "Dic",
            _ => ""
        };

        #endregion
    }
}