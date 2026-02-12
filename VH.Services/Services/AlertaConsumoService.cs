using VH.Services.DTOs.Analytics;
using VH.Services.Entities;
using VH.Services.Interfaces;

namespace VH.Services.Services
{
    /// <summary>
    /// Implementación del servicio de alertas de consumo
    /// </summary>
    public class AlertaConsumoService : IAlertaConsumoService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AlertaConsumoService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        #region Generación de Alertas

        public async Task<List<AlertaConsumo>> EvaluarEntregaAsync(EntregaEPP entrega)
        {
            var alertas = new List<AlertaConsumo>();

            // Obtener el material desde la compra
            var compra = await _unitOfWork.ComprasEPP.GetByIdAsync(entrega.IdCompra, "Material");
            if (compra == null) return alertas;

            var idMaterial = compra.IdMaterial;

            // 1. Evaluar solicitud prematura
            var alertaPrematura = await EvaluarSolicitudPrematuraAsync(entrega.IdEmpleado, idMaterial, entrega.IdEntrega);
            if (alertaPrematura != null) alertas.Add(alertaPrematura);

            // 2. Evaluar exceso de cantidad
            var alertaCantidad = await EvaluarExcesoCantidadAsync(entrega.IdEmpleado, idMaterial, entrega.CantidadEntregada, entrega.IdEntrega);
            if (alertaCantidad != null) alertas.Add(alertaCantidad);

            // 3. Evaluar frecuencia mensual
            var alertaFrecuencia = await EvaluarExcesoFrecuenciaAsync(entrega.IdEmpleado, idMaterial);
            if (alertaFrecuencia != null) alertas.Add(alertaFrecuencia);

            return alertas;
        }

        public async Task<List<AlertaConsumo>> EvaluarRequisicionAsync(RequisicionEPP requisicion)
        {
            var alertas = new List<AlertaConsumo>();

            // Cargar detalles si no están cargados
            var detalles = await _unitOfWork.RequisicionesEPPDetalle.FindAsync(
                d => d.IdRequisicion == requisicion.IdRequisicion);

            foreach (var detalle in detalles)
            {
                // Evaluar cada material solicitado
                var alertaPrematura = await EvaluarSolicitudPrematuraAsync(
                    requisicion.IdEmpleadoRecibe,
                    detalle.IdMaterial,
                    idRequisicion: requisicion.IdRequisicion);

                if (alertaPrematura != null) alertas.Add(alertaPrematura);
            }

            return alertas;
        }

        public async Task<AlertaConsumo?> EvaluarSolicitudPrematuraAsync(int idEmpleado, int idMaterial, int? idEntrega = null, int? idRequisicion = null)
        {
            // Obtener configuración del material
            var config = await GetConfiguracionMaterialAsync(idMaterial);
            if (config == null) return null; // Sin configuración, no evaluar

            // Obtener última entrega del mismo material al mismo empleado
            var entregasAnteriores = await _unitOfWork.EntregasEPP.FindAsync(
                e => e.IdEmpleado == idEmpleado && e.Compra!.IdMaterial == idMaterial,
                "Compra");

            var ultimaEntrega = entregasAnteriores
                .Where(e => idEntrega == null || e.IdEntrega != idEntrega)
                .OrderByDescending(e => e.FechaEntrega)
                .FirstOrDefault();

            if (ultimaEntrega == null) return null; // Primera entrega, no hay comparación

            // Calcular días transcurridos
            var diasTranscurridos = (int)(DateTime.Now - ultimaEntrega.FechaEntrega).TotalDays;
            var umbralDias = (int)(config.VidaUtilDias * config.UmbralAlertaPorcentaje / 100.0);

            if (diasTranscurridos >= umbralDias) return null; // Dentro del rango normal

            // Calcular desviación
            var desviacion = ((config.VidaUtilDias - diasTranscurridos) / (decimal)config.VidaUtilDias) * 100;

            // Determinar severidad
            var severidad = diasTranscurridos < (config.VidaUtilDias * 0.3m)
                ? SeveridadAlerta.Critica
                : diasTranscurridos < (config.VidaUtilDias * 0.5m)
                    ? SeveridadAlerta.Alta
                    : SeveridadAlerta.Media;

            // Obtener datos del empleado y material
            var empleado = await _unitOfWork.Empleados.GetByIdAsync(idEmpleado, "Proyecto");
            var material = await _unitOfWork.MaterialesEPP.GetByIdAsync(idMaterial);

            // Calcular costo estimado
            var costoEstimado = material?.CostoUnitarioEstimado ?? 0;

            var alerta = new AlertaConsumo
            {
                TipoAlerta = TipoAlerta.SolicitudPrematura,
                Severidad = severidad,
                IdEmpleado = idEmpleado,
                IdMaterial = idMaterial,
                IdProyecto = empleado?.IdProyecto,
                IdEntrega = idEntrega,
                IdRequisicion = idRequisicion,
                Descripcion = $"Solicitud prematura de {material?.Nombre ?? "material"}. " +
                              $"Vida útil esperada: {config.VidaUtilDias} días. " +
                              $"Días desde última entrega: {diasTranscurridos}.",
                ValorEsperado = $"{config.VidaUtilDias} días",
                ValorReal = $"{diasTranscurridos} días",
                Desviacion = desviacion,
                CostoEstimado = costoEstimado,
                FechaGeneracion = DateTime.Now,
                EstadoAlerta = EstadoAlerta.Pendiente
            };

            await _unitOfWork.AlertasConsumo.AddAsync(alerta);
            await _unitOfWork.CompleteAsync();

            return alerta;
        }

        public async Task<AlertaConsumo?> EvaluarExcesoCantidadAsync(int idEmpleado, int idMaterial, decimal cantidad, int? idEntrega = null)
        {
            var config = await GetConfiguracionMaterialAsync(idMaterial);
            if (config?.CantidadMaximaPorEntrega == null) return null;

            if (cantidad <= config.CantidadMaximaPorEntrega) return null;

            var empleado = await _unitOfWork.Empleados.GetByIdAsync(idEmpleado, "Proyecto");
            var material = await _unitOfWork.MaterialesEPP.GetByIdAsync(idMaterial);

            var desviacion = ((cantidad - config.CantidadMaximaPorEntrega.Value) / config.CantidadMaximaPorEntrega.Value) * 100;
            var costoExceso = (cantidad - config.CantidadMaximaPorEntrega.Value) * (material?.CostoUnitarioEstimado ?? 0);

            var alerta = new AlertaConsumo
            {
                TipoAlerta = TipoAlerta.ExcesoCantidad,
                Severidad = desviacion > 100 ? SeveridadAlerta.Alta : SeveridadAlerta.Media,
                IdEmpleado = idEmpleado,
                IdMaterial = idMaterial,
                IdProyecto = empleado?.IdProyecto,
                IdEntrega = idEntrega,
                Descripcion = $"Cantidad excesiva de {material?.Nombre ?? "material"}. " +
                              $"Máximo permitido: {config.CantidadMaximaPorEntrega}. Solicitado: {cantidad}.",
                ValorEsperado = $"{config.CantidadMaximaPorEntrega}",
                ValorReal = $"{cantidad}",
                Desviacion = desviacion,
                CostoEstimado = costoExceso,
                FechaGeneracion = DateTime.Now,
                EstadoAlerta = EstadoAlerta.Pendiente
            };

            await _unitOfWork.AlertasConsumo.AddAsync(alerta);
            await _unitOfWork.CompleteAsync();

            return alerta;
        }

        public async Task<AlertaConsumo?> EvaluarExcesoFrecuenciaAsync(int idEmpleado, int idMaterial)
        {
            var config = await GetConfiguracionMaterialAsync(idMaterial);
            if (config?.CantidadMaximaMensual == null) return null;

            // Contar entregas del mes actual
            var inicioMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var entregasMes = await _unitOfWork.EntregasEPP.FindAsync(
                e => e.IdEmpleado == idEmpleado &&
                     e.Compra!.IdMaterial == idMaterial &&
                     e.FechaEntrega >= inicioMes,
                "Compra");

            var totalMes = entregasMes.Sum(e => e.CantidadEntregada);

            if (totalMes <= config.CantidadMaximaMensual) return null;

            // Verificar si ya existe alerta de este tipo este mes
            var alertaExistente = await _unitOfWork.AlertasConsumo.FindAsync(
                a => a.IdEmpleado == idEmpleado &&
                     a.IdMaterial == idMaterial &&
                     a.TipoAlerta == TipoAlerta.ExcesoFrecuencia &&
                     a.FechaGeneracion >= inicioMes);

            if (alertaExistente.Any()) return null; // Ya se generó alerta este mes

            var empleado = await _unitOfWork.Empleados.GetByIdAsync(idEmpleado, "Proyecto");
            var material = await _unitOfWork.MaterialesEPP.GetByIdAsync(idMaterial);

            var desviacion = ((totalMes - config.CantidadMaximaMensual.Value) / config.CantidadMaximaMensual.Value) * 100;

            var alerta = new AlertaConsumo
            {
                TipoAlerta = TipoAlerta.ExcesoFrecuencia,
                Severidad = desviacion > 50 ? SeveridadAlerta.Alta : SeveridadAlerta.Media,
                IdEmpleado = idEmpleado,
                IdMaterial = idMaterial,
                IdProyecto = empleado?.IdProyecto,
                Descripcion = $"Exceso de solicitudes mensuales de {material?.Nombre ?? "material"}. " +
                              $"Máximo mensual: {config.CantidadMaximaMensual}. Acumulado: {totalMes}.",
                ValorEsperado = $"{config.CantidadMaximaMensual} / mes",
                ValorReal = $"{totalMes} / mes",
                Desviacion = desviacion,
                CostoEstimado = (totalMes - config.CantidadMaximaMensual.Value) * (material?.CostoUnitarioEstimado ?? 0),
                FechaGeneracion = DateTime.Now,
                EstadoAlerta = EstadoAlerta.Pendiente
            };

            await _unitOfWork.AlertasConsumo.AddAsync(alerta);
            await _unitOfWork.CompleteAsync();

            return alerta;
        }

        #endregion

        #region Consultas

        public async Task<IEnumerable<AlertaConsumoResponseDto>> GetAlertasAsync(FiltroAlertasDto filtros)
        {
            var alertas = await _unitOfWork.AlertasConsumo.FindAsync(
                a => (!filtros.FechaDesde.HasValue || a.FechaGeneracion >= filtros.FechaDesde) &&
                     (!filtros.FechaHasta.HasValue || a.FechaGeneracion <= filtros.FechaHasta) &&
                     (!filtros.IdProyecto.HasValue || a.IdProyecto == filtros.IdProyecto) &&
                     (!filtros.IdEmpleado.HasValue || a.IdEmpleado == filtros.IdEmpleado) &&
                     (!filtros.IdMaterial.HasValue || a.IdMaterial == filtros.IdMaterial) &&
                     (!filtros.TipoAlerta.HasValue || a.TipoAlerta == filtros.TipoAlerta) &&
                     (!filtros.Severidad.HasValue || a.Severidad == filtros.Severidad) &&
                     (!filtros.Estado.HasValue || a.EstadoAlerta == filtros.Estado) &&
                     (!filtros.SoloPendientes || a.EstadoAlerta == EstadoAlerta.Pendiente) &&
                     (!filtros.SoloCriticas || a.Severidad == SeveridadAlerta.Critica),
                "Empleado.Proyecto,Material,UsuarioReviso");

            return alertas.Select(MapToResponseDto).OrderByDescending(a => a.FechaGeneracion);
        }

        public async Task<AlertaConsumoResponseDto?> GetAlertaByIdAsync(int id)
        {
            var alerta = await _unitOfWork.AlertasConsumo.GetByIdAsync(id, "Empleado.Proyecto,Material,UsuarioReviso");
            return alerta != null ? MapToResponseDto(alerta) : null;
        }

        public async Task<IEnumerable<AlertaConsumoResponseDto>> GetAlertasPendientesEmpleadoAsync(int idEmpleado)
        {
            var alertas = await _unitOfWork.AlertasConsumo.FindAsync(
                a => a.IdEmpleado == idEmpleado && a.EstadoAlerta == EstadoAlerta.Pendiente,
                "Material");

            return alertas.Select(MapToResponseDto).OrderByDescending(a => a.Severidad);
        }

        public async Task<IEnumerable<AlertaConsumoResponseDto>> GetAlertasProyectoAsync(int idProyecto, bool soloPendientes = false)
        {
            var alertas = await _unitOfWork.AlertasConsumo.FindAsync(
                a => a.IdProyecto == idProyecto && (!soloPendientes || a.EstadoAlerta == EstadoAlerta.Pendiente),
                "Empleado,Material");

            return alertas.Select(MapToResponseDto).OrderByDescending(a => a.FechaGeneracion);
        }

        public async Task<ResumenAlertasDto> GetResumenAlertasAsync(int? idProyecto = null)
        {
            var hoy = DateTime.Today;
            var inicioSemana = hoy.AddDays(-(int)hoy.DayOfWeek);
            var inicioSemanaAnterior = inicioSemana.AddDays(-7);

            var todasAlertas = await _unitOfWork.AlertasConsumo.FindAsync(
                a => !idProyecto.HasValue || a.IdProyecto == idProyecto);

            var alertasList = todasAlertas.ToList();

            return new ResumenAlertasDto
            {
                TotalPendientes = alertasList.Count(a => a.EstadoAlerta == EstadoAlerta.Pendiente),
                TotalCriticas = alertasList.Count(a => a.Severidad == SeveridadAlerta.Critica && a.EstadoAlerta == EstadoAlerta.Pendiente),
                TotalAltas = alertasList.Count(a => a.Severidad == SeveridadAlerta.Alta && a.EstadoAlerta == EstadoAlerta.Pendiente),
                TotalMedias = alertasList.Count(a => a.Severidad == SeveridadAlerta.Media && a.EstadoAlerta == EstadoAlerta.Pendiente),
                TotalBajas = alertasList.Count(a => a.Severidad == SeveridadAlerta.Baja && a.EstadoAlerta == EstadoAlerta.Pendiente),
                RevisadasHoy = alertasList.Count(a => a.FechaRevision?.Date == hoy),
                GeneradasHoy = alertasList.Count(a => a.FechaGeneracion.Date == hoy),
                CostoPotencialTotal = alertasList.Where(a => a.EstadoAlerta == EstadoAlerta.Pendiente).Sum(a => a.CostoEstimado ?? 0),
                SolicitudesPrematuras = alertasList.Count(a => a.TipoAlerta == TipoAlerta.SolicitudPrematura && a.EstadoAlerta == EstadoAlerta.Pendiente),
                ExcesosFrecuencia = alertasList.Count(a => a.TipoAlerta == TipoAlerta.ExcesoFrecuencia && a.EstadoAlerta == EstadoAlerta.Pendiente),
                ExcesosCantidad = alertasList.Count(a => a.TipoAlerta == TipoAlerta.ExcesoCantidad && a.EstadoAlerta == EstadoAlerta.Pendiente),
                PatronesAnomalos = alertasList.Count(a => a.TipoAlerta == TipoAlerta.PatronAnomalo && a.EstadoAlerta == EstadoAlerta.Pendiente),
                DesviacionesPresupuesto = alertasList.Count(a => a.TipoAlerta == TipoAlerta.DesviacionPresupuestal && a.EstadoAlerta == EstadoAlerta.Pendiente),
                TopConsumidores = alertasList.Count(a => a.TipoAlerta == TipoAlerta.TopConsumidor && a.EstadoAlerta == EstadoAlerta.Pendiente),
                AlertasUltimaSemana = alertasList.Count(a => a.FechaGeneracion >= inicioSemana),
                AlertasSemanaAnterior = alertasList.Count(a => a.FechaGeneracion >= inicioSemanaAnterior && a.FechaGeneracion < inicioSemana),
                VariacionSemanal = CalcularVariacionPorcentaje(
                    alertasList.Count(a => a.FechaGeneracion >= inicioSemanaAnterior && a.FechaGeneracion < inicioSemana),
                    alertasList.Count(a => a.FechaGeneracion >= inicioSemana))
            };
        }

        #endregion

        #region Gestión de Estados

        public async Task<bool> RevisarAlertaAsync(int idAlerta, string idUsuario, EstadoAlerta nuevoEstado, string? observaciones)
        {
            var alerta = await _unitOfWork.AlertasConsumo.GetByIdAsync(idAlerta);
            if (alerta == null) return false;

            alerta.EstadoAlerta = nuevoEstado;
            alerta.IdUsuarioReviso = idUsuario;
            alerta.FechaRevision = DateTime.Now;
            alerta.Observaciones = observaciones;

            _unitOfWork.AlertasConsumo.Update(alerta);
            return await _unitOfWork.CompleteAsync() > 0;
        }

        public async Task<int> RevisarAlertasMasivoAsync(IEnumerable<int> idsAlertas, string idUsuario, EstadoAlerta nuevoEstado, string? observaciones)
        {
            var contador = 0;
            foreach (var id in idsAlertas)
            {
                if (await RevisarAlertaAsync(id, idUsuario, nuevoEstado, observaciones))
                    contador++;
            }
            return contador;
        }

        #endregion

        #region Configuración

        public async Task<ConfiguracionMaterialEPP?> GetConfiguracionMaterialAsync(int idMaterial)
        {
            var configs = await _unitOfWork.ConfiguracionesMaterialEPP.FindAsync(
                c => c.IdMaterial == idMaterial && c.Activo);
            return configs.FirstOrDefault();
        }

        public async Task<ConfiguracionMaterialEPP> GuardarConfiguracionMaterialAsync(ConfiguracionMaterialRequestDto dto)
        {
            var existente = await GetConfiguracionMaterialAsync(dto.IdMaterial);

            if (existente != null)
            {
                existente.VidaUtilDias = dto.VidaUtilDias;
                existente.FrecuenciaMinimaDias = dto.FrecuenciaMinimaDias;
                existente.CantidadMaximaMensual = dto.CantidadMaximaMensual;
                existente.CantidadMaximaPorEntrega = dto.CantidadMaximaPorEntrega;
                existente.RequiereDevolucion = dto.RequiereDevolucion;
                existente.UmbralAlertaPorcentaje = dto.UmbralAlertaPorcentaje;

                _unitOfWork.ConfiguracionesMaterialEPP.Update(existente);
                await _unitOfWork.CompleteAsync();
                return existente;
            }

            var nueva = new ConfiguracionMaterialEPP
            {
                IdMaterial = dto.IdMaterial,
                VidaUtilDias = dto.VidaUtilDias,
                FrecuenciaMinimaDias = dto.FrecuenciaMinimaDias,
                CantidadMaximaMensual = dto.CantidadMaximaMensual,
                CantidadMaximaPorEntrega = dto.CantidadMaximaPorEntrega,
                RequiereDevolucion = dto.RequiereDevolucion,
                UmbralAlertaPorcentaje = dto.UmbralAlertaPorcentaje,
                Activo = true
            };

            await _unitOfWork.ConfiguracionesMaterialEPP.AddAsync(nueva);
            await _unitOfWork.CompleteAsync();
            return nueva;
        }

        public async Task<IEnumerable<ConfiguracionMaterialResponseDto>> GetTodasConfiguracionesAsync()
        {
            var configs = await _unitOfWork.ConfiguracionesMaterialEPP.FindAsync(
                c => c.Activo,
                "Material.UnidadMedida");

            return configs.Select(c => new ConfiguracionMaterialResponseDto
            {
                IdConfiguracion = c.IdConfiguracion,
                IdMaterial = c.IdMaterial,
                NombreMaterial = c.Material?.Nombre ?? "",
                UnidadMedida = c.Material?.UnidadMedida?.Abreviatura ?? "",
                VidaUtilDias = c.VidaUtilDias,
                FrecuenciaMinimaDias = c.FrecuenciaMinimaDias,
                CantidadMaximaMensual = c.CantidadMaximaMensual,
                CantidadMaximaPorEntrega = c.CantidadMaximaPorEntrega,
                RequiereDevolucion = c.RequiereDevolucion,
                UmbralAlertaPorcentaje = c.UmbralAlertaPorcentaje,
                Activo = c.Activo
            });
        }

        #endregion

        #region Helpers

        private AlertaConsumoResponseDto MapToResponseDto(AlertaConsumo alerta)
        {
            return new AlertaConsumoResponseDto
            {
                IdAlerta = alerta.IdAlerta,
                TipoAlerta = alerta.TipoAlerta,
                TipoAlertaTexto = GetTipoAlertaTexto(alerta.TipoAlerta),
                Severidad = alerta.Severidad,
                SeveridadTexto = alerta.Severidad.ToString(),
                ColorSeveridad = GetColorSeveridad(alerta.Severidad),
                IconoTipo = GetIconoTipo(alerta.TipoAlerta),
                IdEmpleado = alerta.IdEmpleado,
                NumeroNomina = alerta.Empleado?.NumeroNomina ?? "",
                NombreEmpleado = alerta.Empleado != null
                    ? $"{alerta.Empleado.Nombre} {alerta.Empleado.ApellidoPaterno}"
                    : "",
                PuestoEmpleado = alerta.Empleado?.Puesto ?? "",
                IdMaterial = alerta.IdMaterial,
                NombreMaterial = alerta.Material?.Nombre ?? "",
                IdProyecto = alerta.IdProyecto,
                NombreProyecto = alerta.Empleado?.Proyecto?.Nombre ?? "",
                Descripcion = alerta.Descripcion,
                ValorEsperado = alerta.ValorEsperado,
                ValorReal = alerta.ValorReal,
                Desviacion = alerta.Desviacion,
                CostoEstimado = alerta.CostoEstimado,
                IdEntrega = alerta.IdEntrega,
                IdRequisicion = alerta.IdRequisicion,
                EstadoAlerta = alerta.EstadoAlerta,
                EstadoTexto = alerta.EstadoAlerta.ToString(),
                ColorEstado = GetColorEstado(alerta.EstadoAlerta),
                FechaGeneracion = alerta.FechaGeneracion,
                FechaRevision = alerta.FechaRevision,
                IdUsuarioReviso = alerta.IdUsuarioReviso,
                NombreUsuarioReviso = alerta.UsuarioReviso?.UserName,
                Observaciones = alerta.Observaciones
            };
        }

        private static string GetTipoAlertaTexto(TipoAlerta tipo) => tipo switch
        {
            TipoAlerta.SolicitudPrematura => "Solicitud Prematura",
            TipoAlerta.ExcesoFrecuencia => "Exceso de Frecuencia",
            TipoAlerta.ExcesoCantidad => "Exceso de Cantidad",
            TipoAlerta.PatronAnomalo => "Patrón Anómalo",
            TipoAlerta.DesviacionPresupuestal => "Desviación Presupuestal",
            TipoAlerta.TopConsumidor => "Top Consumidor",
            _ => tipo.ToString()
        };

        private static string GetColorSeveridad(SeveridadAlerta severidad) => severidad switch
        {
            SeveridadAlerta.Critica => "danger",
            SeveridadAlerta.Alta => "warning",
            SeveridadAlerta.Media => "info",
            SeveridadAlerta.Baja => "secondary",
            _ => "light"
        };

        private static string GetColorEstado(EstadoAlerta estado) => estado switch
        {
            EstadoAlerta.Pendiente => "warning",
            EstadoAlerta.EnRevision => "info",
            EstadoAlerta.Descartada => "secondary",
            EstadoAlerta.Confirmada => "danger",
            EstadoAlerta.Resuelta => "success",
            _ => "light"
        };

        private static string GetIconoTipo(TipoAlerta tipo) => tipo switch
        {
            TipoAlerta.SolicitudPrematura => "bi-clock-history",
            TipoAlerta.ExcesoFrecuencia => "bi-arrow-repeat",
            TipoAlerta.ExcesoCantidad => "bi-box-seam",
            TipoAlerta.PatronAnomalo => "bi-graph-up-arrow",
            TipoAlerta.DesviacionPresupuestal => "bi-currency-dollar",
            TipoAlerta.TopConsumidor => "bi-trophy",
            _ => "bi-exclamation-triangle"
        };

        private static decimal CalcularVariacionPorcentaje(int anterior, int actual)
        {
            if (anterior == 0) return actual > 0 ? 100 : 0;
            return ((actual - anterior) / (decimal)anterior) * 100;
        }

        #endregion
    }
}