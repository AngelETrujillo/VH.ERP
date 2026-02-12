using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VH.Services.Entities
{
    /// <summary>
    /// Registro de alertas generadas por el sistema de detección de anomalías.
    /// Permite seguimiento y auditoría de posibles fugas de material.
    /// </summary>
    [Table("AlertasConsumo")]
    public class AlertaConsumo
    {
        [Key]
        public int IdAlerta { get; set; }

        /// <summary>
        /// Tipo de anomalía detectada
        /// </summary>
        [Required]
        public TipoAlerta TipoAlerta { get; set; }

        /// <summary>
        /// Nivel de severidad de la alerta
        /// </summary>
        [Required]
        public SeveridadAlerta Severidad { get; set; }

        /// <summary>
        /// Empleado relacionado con la alerta
        /// </summary>
        [Required]
        public int IdEmpleado { get; set; }

        /// <summary>
        /// Material involucrado en la alerta
        /// </summary>
        public int? IdMaterial { get; set; }

        /// <summary>
        /// Proyecto donde ocurrió la anomalía
        /// </summary>
        public int? IdProyecto { get; set; }

        /// <summary>
        /// Entrega que disparó la alerta (si aplica)
        /// </summary>
        public int? IdEntrega { get; set; }

        /// <summary>
        /// Requisición que disparó la alerta (si aplica)
        /// </summary>
        public int? IdRequisicion { get; set; }

        /// <summary>
        /// Descripción detallada de la anomalía detectada
        /// </summary>
        [Required]
        [MaxLength(1000)]
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>
        /// Valor esperado según configuración (ej: "90 días")
        /// </summary>
        [MaxLength(100)]
        public string ValorEsperado { get; set; } = string.Empty;

        /// <summary>
        /// Valor real encontrado (ej: "15 días")
        /// </summary>
        [MaxLength(100)]
        public string ValorReal { get; set; } = string.Empty;

        /// <summary>
        /// Porcentaje de desviación respecto al valor esperado
        /// </summary>
        public decimal Desviacion { get; set; }

        /// <summary>
        /// Costo estimado de la pérdida potencial
        /// </summary>
        public decimal? CostoEstimado { get; set; }

        /// <summary>
        /// Fecha y hora en que se generó la alerta
        /// </summary>
        [Required]
        public DateTime FechaGeneracion { get; set; } = DateTime.Now;

        /// <summary>
        /// Fecha en que se revisó la alerta
        /// </summary>
        public DateTime? FechaRevision { get; set; }

        /// <summary>
        /// Usuario que revisó la alerta
        /// </summary>
        public string? IdUsuarioReviso { get; set; }

        /// <summary>
        /// Estado actual de la alerta
        /// </summary>
        [Required]
        public EstadoAlerta EstadoAlerta { get; set; } = EstadoAlerta.Pendiente;

        /// <summary>
        /// Observaciones del revisor
        /// </summary>
        [MaxLength(1000)]
        public string? Observaciones { get; set; }

        // ===== PROPIEDADES DE NAVEGACIÓN =====

        [ForeignKey("IdEmpleado")]
        public virtual Empleado? Empleado { get; set; }

        [ForeignKey("IdMaterial")]
        public virtual MaterialEPP? Material { get; set; }

        [ForeignKey("IdProyecto")]
        public virtual Proyecto? Proyecto { get; set; }

        [ForeignKey("IdEntrega")]
        public virtual EntregaEPP? Entrega { get; set; }

        [ForeignKey("IdRequisicion")]
        public virtual RequisicionEPP? Requisicion { get; set; }

        [ForeignKey("IdUsuarioReviso")]
        public virtual Usuario? UsuarioReviso { get; set; }
    }

    /// <summary>
    /// Tipos de alerta que el sistema puede generar
    /// </summary>
    public enum TipoAlerta
    {
        /// <summary>
        /// Solicitud antes de cumplir la vida útil esperada
        /// </summary>
        SolicitudPrematura = 1,

        /// <summary>
        /// Demasiadas solicitudes en un período corto
        /// </summary>
        ExcesoFrecuencia = 2,

        /// <summary>
        /// Cantidad inusual en una sola entrega
        /// </summary>
        ExcesoCantidad = 3,

        /// <summary>
        /// Comportamiento atípico detectado por algoritmo
        /// </summary>
        PatronAnomalo = 4,

        /// <summary>
        /// Proyecto excede presupuesto de EPP
        /// </summary>
        DesviacionPresupuestal = 5,

        /// <summary>
        /// Empleado en ranking de máximos consumidores
        /// </summary>
        TopConsumidor = 6
    }

    /// <summary>
    /// Nivel de severidad de las alertas
    /// </summary>
    public enum SeveridadAlerta
    {
        /// <summary>
        /// Informativa, solo para monitoreo
        /// </summary>
        Baja = 1,

        /// <summary>
        /// Requiere revisión eventual
        /// </summary>
        Media = 2,

        /// <summary>
        /// Requiere acción pronta
        /// </summary>
        Alta = 3,

        /// <summary>
        /// Requiere acción inmediata
        /// </summary>
        Critica = 4
    }

    /// Estados posibles de una alerta
    public enum EstadoAlerta
    {
        /// Alerta sin revisar
        Pendiente = 1,

        /// En proceso de investigación
        EnRevision = 2,

        /// Falso positivo descartado
        Descartada = 3,

        /// Anomalía confirmada
        Confirmada = 4,

        /// Caso cerrado con acciones tomadas
        Resuelta = 5
    }
}