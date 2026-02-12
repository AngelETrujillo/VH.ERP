using System;
using System.Collections.Generic;

namespace VH.Services.DTOs.Analytics
{
    // ===== KPIs EJECUTIVOS =====
    public class KPIsEjecutivosDto
    {
        // Consumo
        public decimal ConsumoMesActual { get; set; }
        public decimal ConsumoMesAnterior { get; set; }
        public decimal VariacionConsumoMensual { get; set; }
        public decimal ConsumoAcumuladoAnio { get; set; }

        // Alertas
        public int AlertasCriticas { get; set; }
        public int AlertasAltas { get; set; }
        public int AlertasPendientes { get; set; }
        public decimal CostoPerdidaPotencial { get; set; }

        // Empleados
        public int EmpleadosConAlertas { get; set; }
        public int TotalEmpleadosActivos { get; set; }
        public decimal PorcentajeEmpleadosConAlertas { get; set; }

        // Proyectos
        public int ProyectosSobrePresupuesto { get; set; }
        public int TotalProyectosActivos { get; set; }
    }

    // ===== RANKING DE CONSUMIDORES =====
    public class RankingConsumidorDto
    {
        public int Posicion { get; set; }
        public int IdEmpleado { get; set; }
        public string NumeroNomina { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string Puesto { get; set; } = string.Empty;
        public string NombreProyecto { get; set; } = string.Empty;

        // Métricas de consumo
        public int TotalEntregas { get; set; }
        public decimal TotalUnidades { get; set; }
        public decimal CostoTotal { get; set; }

        // Comparación con promedio del puesto
        public decimal PromedioGrupoPuesto { get; set; }
        public decimal DesviacionPorcentaje { get; set; }

        // Indicadores de riesgo
        public decimal PuntuacionRiesgo { get; set; }
        public int AlertasActivas { get; set; }
        public string NivelRiesgo { get; set; } = string.Empty; // "Crítico", "Alto", "Medio", "Bajo", "Normal"
        public string ColorIndicador { get; set; } = string.Empty; // Para UI: "danger", "warning", "info", "success"

        // Empate
        public bool EsEmpate { get; set; }
    }
    public class RankingConsumoResponseDto
    {
        public List<RankingConsumidorDto> TopConsumidores { get; set; } = new();
        public List<RankingConsumidorDto> BottomConsumidores { get; set; } = new();
        public int TotalEmpleadosAnalizados { get; set; }
        public decimal PromedioGeneral { get; set; }
        public DateTime FechaCalculo { get; set; }
        public int Anio { get; set; }
        public int Mes { get; set; }
    }

    // ===== CONSUMO POR PROYECTO =====
    public class ConsumoProyectoDto
    {
        public int IdProyecto { get; set; }
        public string NombreProyecto { get; set; } = string.Empty;
        public string TipoObra { get; set; } = string.Empty;

        // Métricas actuales
        public decimal CostoTotal { get; set; }
        public int TotalEntregas { get; set; }
        public int TotalEmpleados { get; set; }
        public decimal CostoPromedioPorEmpleado { get; set; }

        // Presupuesto
        public decimal PresupuestoAsignado { get; set; }
        public decimal DesviacionPresupuesto { get; set; }
        public bool SobrePresupuesto { get; set; }

        // Alertas
        public int AlertasCriticas { get; set; }
        public int TotalAlertas { get; set; }

        // Para gráficos
        public string ColorBarra { get; set; } = string.Empty;
    }

    // ===== HISTORIAL DE EMPLEADO =====
    public class PerfilConsumoEmpleadoDto
    {
        // Datos del empleado
        public int IdEmpleado { get; set; }
        public string NumeroNomina { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string Puesto { get; set; } = string.Empty;
        public string NombreProyecto { get; set; } = string.Empty;
        public DateTime? FechaIngreso { get; set; }
        public int DiasEnEmpresa { get; set; }

        // Score de riesgo
        public decimal PuntuacionRiesgo { get; set; }
        public string NivelRiesgo { get; set; } = string.Empty;
        public DateTime? FechaUltimoCalculoRiesgo { get; set; }

        // Resumen de consumo
        public decimal CostoTotalHistorico { get; set; }
        public int TotalEntregasHistorico { get; set; }
        public decimal PromedioMensual { get; set; }

        // Comparación con grupo
        public decimal PromedioGrupoPuesto { get; set; }
        public decimal DesviacionVsGrupo { get; set; }

        // Alertas
        public int AlertasPendientes { get; set; }
        public int AlertasConfirmadas { get; set; }
        public int AlertasDescartadas { get; set; }

        // Detalle por período
        public List<ConsumoMensualDto> HistorialMensual { get; set; } = new();

        // Materiales más solicitados
        public List<MaterialFrecuenteDto> MaterialesFrecuentes { get; set; } = new();

        // Últimas entregas
        public List<EntregaResumenDto> UltimasEntregas { get; set; } = new();

        // Alertas recientes
        public List<AlertaResumenDto> AlertasRecientes { get; set; } = new();
    }

    public class ConsumoMensualDto
    {
        public int Anio { get; set; }
        public int Mes { get; set; }
        public string Periodo { get; set; } = string.Empty; // "Ene 2026"
        public int TotalEntregas { get; set; }
        public decimal CostoTotal { get; set; }
        public int AlertasGeneradas { get; set; }
        public decimal PuntuacionRiesgo { get; set; }
    }

    public class MaterialFrecuenteDto
    {
        public int IdMaterial { get; set; }
        public string NombreMaterial { get; set; } = string.Empty;
        public int VecesSolicitado { get; set; }
        public decimal CantidadTotal { get; set; }
        public decimal CostoTotal { get; set; }
        public int? VidaUtilEsperada { get; set; }
        public decimal PromedioEntreSolicitudes { get; set; } // Días promedio entre solicitudes
    }

    public class EntregaResumenDto
    {
        public int IdEntrega { get; set; }
        public DateTime FechaEntrega { get; set; }
        public string NombreMaterial { get; set; } = string.Empty;
        public decimal Cantidad { get; set; }
        public decimal Costo { get; set; }
        public bool GeneroAlerta { get; set; }
    }

    public class AlertaResumenDto
    {
        public int IdAlerta { get; set; }
        public DateTime FechaGeneracion { get; set; }
        public string TipoAlerta { get; set; } = string.Empty;
        public string Severidad { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string ColorSeveridad { get; set; } = string.Empty;
    }

    // ===== TENDENCIAS =====
    public class TendenciaConsumoDto
    {
        public List<PuntoTendenciaDto> Puntos { get; set; } = new();
        public decimal PromedioHistorico { get; set; }
        public decimal TendenciaPorcentaje { get; set; } // Positivo = creciente
        public string Direccion { get; set; } = string.Empty; // "Ascendente", "Descendente", "Estable"
    }

    public class PuntoTendenciaDto
    {
        public int Anio { get; set; }
        public int Mes { get; set; }
        public string Etiqueta { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public decimal? PresupuestoReferencia { get; set; }
    }

    // ===== HEATMAP FRECUENCIA =====
    public class HeatmapFrecuenciaDto
    {
        public List<FilaHeatmapDto> Filas { get; set; } = new();
        public List<string> ColumnasEtiquetas { get; set; } = new(); // Materiales
        public decimal ValorMaximo { get; set; }
    }

    public class FilaHeatmapDto
    {
        public int IdEmpleado { get; set; }
        public string NombreEmpleado { get; set; } = string.Empty;
        public List<CeldaHeatmapDto> Celdas { get; set; } = new();
    }

    public class CeldaHeatmapDto
    {
        public int IdMaterial { get; set; }
        public decimal Valor { get; set; }
        public string Color { get; set; } = string.Empty;
        public string Tooltip { get; set; } = string.Empty;
    }
}