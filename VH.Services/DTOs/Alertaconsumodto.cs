using System;
using System.ComponentModel.DataAnnotations;
using VH.Services.Entities;

namespace VH.Services.DTOs.Analytics
{
    // ===== REQUEST DTOs =====
    public class FiltroAlertasDto
    {
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public int? IdProyecto { get; set; }
        public int? IdEmpleado { get; set; }
        public int? IdMaterial { get; set; }
        public TipoAlerta? TipoAlerta { get; set; }
        public SeveridadAlerta? Severidad { get; set; }
        public EstadoAlerta? Estado { get; set; }
        public bool SoloPendientes { get; set; } = false;
        public bool SoloCriticas { get; set; } = false;
    }
    public record RevisarAlertaRequestDto(
        [Required]
        EstadoAlerta NuevoEstado,

        [MaxLength(1000)]
        string? Observaciones
    );
    public record ConfiguracionMaterialRequestDto(
        [Required(ErrorMessage = "El material es obligatorio")]
        [Range(1, int.MaxValue)]
        int IdMaterial,

        [Required(ErrorMessage = "La vida útil es obligatoria")]
        [Range(1, 3650, ErrorMessage = "La vida útil debe estar entre 1 y 3650 días")]
        int VidaUtilDias,

        [Required(ErrorMessage = "La frecuencia mínima es obligatoria")]
        [Range(1, 365, ErrorMessage = "La frecuencia debe estar entre 1 y 365 días")]
        int FrecuenciaMinimaDias,

        [Range(0.01, 10000)]
        decimal? CantidadMaximaMensual,

        [Range(0.01, 1000)]
        decimal? CantidadMaximaPorEntrega,

        bool RequiereDevolucion,

        [Range(1, 100)]
        int UmbralAlertaPorcentaje = 70
    );

    // ===== RESPONSE DTOs =====
    public class AlertaConsumoResponseDto
    {
        public int IdAlerta { get; set; }

        // Tipo y severidad
        public TipoAlerta TipoAlerta { get; set; }
        public string TipoAlertaTexto { get; set; } = string.Empty;
        public SeveridadAlerta Severidad { get; set; }
        public string SeveridadTexto { get; set; } = string.Empty;
        public string ColorSeveridad { get; set; } = string.Empty;
        public string IconoTipo { get; set; } = string.Empty;

        // Empleado
        public int IdEmpleado { get; set; }
        public string NumeroNomina { get; set; } = string.Empty;
        public string NombreEmpleado { get; set; } = string.Empty;
        public string PuestoEmpleado { get; set; } = string.Empty;

        // Material
        public int? IdMaterial { get; set; }
        public string NombreMaterial { get; set; } = string.Empty;

        // Proyecto
        public int? IdProyecto { get; set; }
        public string NombreProyecto { get; set; } = string.Empty;

        // Detalle de la anomalía
        public string Descripcion { get; set; } = string.Empty;
        public string ValorEsperado { get; set; } = string.Empty;
        public string ValorReal { get; set; } = string.Empty;
        public decimal Desviacion { get; set; }
        public decimal? CostoEstimado { get; set; }

        // Referencias
        public int? IdEntrega { get; set; }
        public int? IdRequisicion { get; set; }

        // Estado
        public EstadoAlerta EstadoAlerta { get; set; }
        public string EstadoTexto { get; set; } = string.Empty;
        public string ColorEstado { get; set; } = string.Empty;

        // Fechas
        public DateTime FechaGeneracion { get; set; }
        public DateTime? FechaRevision { get; set; }

        // Revisor
        public string? IdUsuarioReviso { get; set; }
        public string? NombreUsuarioReviso { get; set; }
        public string? Observaciones { get; set; }

        // Calculados
        public int DiasDesdeGeneracion => (int)(DateTime.Now - FechaGeneracion).TotalDays;
        public bool EsCritica => Severidad == SeveridadAlerta.Critica;
        public bool RequiereAccion => EstadoAlerta == EstadoAlerta.Pendiente &&
                                       (Severidad == SeveridadAlerta.Alta || Severidad == SeveridadAlerta.Critica);
    }
    public class ResumenAlertasDto
    {
        public int TotalPendientes { get; set; }
        public int TotalCriticas { get; set; }
        public int TotalAltas { get; set; }
        public int TotalMedias { get; set; }
        public int TotalBajas { get; set; }

        public int RevisadasHoy { get; set; }
        public int GeneradasHoy { get; set; }

        public decimal CostoPotencialTotal { get; set; }

        // Por tipo
        public int SolicitudesPrematuras { get; set; }
        public int ExcesosFrecuencia { get; set; }
        public int ExcesosCantidad { get; set; }
        public int PatronesAnomalos { get; set; }
        public int DesviacionesPresupuesto { get; set; }
        public int TopConsumidores { get; set; }

        // Tendencia
        public int AlertasUltimaSemana { get; set; }
        public int AlertasSemanaAnterior { get; set; }
        public decimal VariacionSemanal { get; set; }
    }
    public class ConfiguracionMaterialResponseDto
    {
        public int IdConfiguracion { get; set; }
        public int IdMaterial { get; set; }
        public string NombreMaterial { get; set; } = string.Empty;
        public string UnidadMedida { get; set; } = string.Empty;

        public int VidaUtilDias { get; set; }
        public int FrecuenciaMinimaDias { get; set; }
        public decimal? CantidadMaximaMensual { get; set; }
        public decimal? CantidadMaximaPorEntrega { get; set; }
        public bool RequiereDevolucion { get; set; }
        public int UmbralAlertaPorcentaje { get; set; }
        public bool Activo { get; set; }

        // Estadísticas de uso
        public int AlertasGeneradas { get; set; }
        public decimal PromedioSolicitudesMes { get; set; }
    }

    // ===== DTOs PARA PUESTO =====

    public record PuestoRequestDto(
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [MaxLength(100)]
        string Nombre,

        [MaxLength(500)]
        string? Descripcion,

        NivelRiesgoEPP NivelRiesgoEPP = NivelRiesgoEPP.Medio,

        bool Activo = true
    );

    public class PuestoResponseDto
    {
        public int IdPuesto { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public NivelRiesgoEPP NivelRiesgoEPP { get; set; }
        public string NivelRiesgoTexto { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public int TotalEmpleados { get; set; }
    }
}