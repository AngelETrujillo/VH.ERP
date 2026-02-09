using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VH.Services.Entities
{
    [Table("RequisicionesEPP")]
    public class RequisicionEPP
    {
        [Key]
        public int IdRequisicion { get; set; }

        [Required]
        [MaxLength(20)]
        public string NumeroRequisicion { get; set; } = string.Empty;

        // ===== SOLICITANTE =====

        [Required]
        public string IdUsuarioSolicita { get; set; } = string.Empty;

        [Required]
        public int IdEmpleadoRecibe { get; set; }

        [Required]
        public int IdAlmacen { get; set; }

        [MaxLength(500)]
        public string? Justificacion { get; set; }

        // ===== ESTADO =====

        [Required]
        public EstadoRequisicion EstadoRequisicion { get; set; } = EstadoRequisicion.Pendiente;

        // ===== APROBACIÓN =====

        public string? IdUsuarioAprueba { get; set; }

        public DateTime? FechaAprobacion { get; set; }

        [MaxLength(500)]
        public string? MotivoRechazo { get; set; }

        // ===== ENTREGA =====

        public string? IdUsuarioEntrega { get; set; }

        public DateTime? FechaEntrega { get; set; }

        [MaxLength(500000)]
        public string? FirmaDigital { get; set; }

        [MaxLength(300)]
        public string? FotoEvidencia { get; set; }

        [MaxLength(500)]
        public string? Observaciones { get; set; }

        // ===== AUDITORÍA =====

        [Required]
        public DateTime FechaSolicitud { get; set; } = DateTime.Now;

        // ===== NAVEGACIÓN =====

        [ForeignKey("IdUsuarioSolicita")]
        public virtual Usuario? UsuarioSolicita { get; set; }

        [ForeignKey("IdEmpleadoRecibe")]
        public virtual Empleado? EmpleadoRecibe { get; set; }

        [ForeignKey("IdAlmacen")]
        public virtual Almacen? Almacen { get; set; }

        [ForeignKey("IdUsuarioAprueba")]
        public virtual Usuario? UsuarioAprueba { get; set; }

        [ForeignKey("IdUsuarioEntrega")]
        public virtual Usuario? UsuarioEntrega { get; set; }

        public virtual ICollection<RequisicionEPPDetalle> Detalles { get; set; } = new List<RequisicionEPPDetalle>();
    }
}