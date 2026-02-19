using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VH.Services.Entities
{
    [Table("AlertasConsumo")]
    public class AlertaConsumo
    {
        [Key]
        public int IdAlerta { get; set; }
        [Required]
        public TipoAlerta TipoAlerta { get; set; }
        [Required]
        public SeveridadAlerta Severidad { get; set; }
        [Required]
        public int IdEmpleado { get; set; }
        public int? IdMaterial { get; set; }
        public int? IdProyecto { get; set; }
        public int? IdEntrega { get; set; }
        public int? IdRequisicion { get; set; }
        [Required]
        [MaxLength(1000)]
        public string Descripcion { get; set; } = string.Empty;
        [MaxLength(100)]
        public string ValorEsperado { get; set; } = string.Empty;
        [MaxLength(100)]
        public string ValorReal { get; set; } = string.Empty;
        public decimal Desviacion { get; set; }
        public decimal? CostoEstimado { get; set; }
        [Required]
        public DateTime FechaGeneracion { get; set; } = DateTime.Now;
        public DateTime? FechaRevision { get; set; }
        public string? IdUsuarioReviso { get; set; }
        [Required]
        public EstadoAlerta EstadoAlerta { get; set; } = EstadoAlerta.Pendiente;
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
    public enum TipoAlerta
    {
        SolicitudPrematura = 1,
        ExcesoFrecuencia = 2,
        ExcesoCantidad = 3,
        PatronAnomalo = 4,
        DesviacionPresupuestal = 5,
        TopConsumidor = 6
    }
    public enum SeveridadAlerta
    {
        Baja = 1,
        Media = 2,
        Alta = 3,
        Critica = 4
    }

    /// Estados posibles de una alerta
    public enum EstadoAlerta
    {
        Pendiente = 1,
        EnRevision = 2,
        Descartada = 3,
        Confirmada = 4,
        Resuelta = 5
    }
}