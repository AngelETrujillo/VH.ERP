using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VH.Services.Entities
{
    /// <summary>
    /// Acto de entrega a una persona: lo que un obrero recibió de una requisición,
    /// con su firma.
    ///
    /// La firma vivía en la cabecera de la requisición, lo que sólo funciona
    /// mientras el documento sea de una sola persona. Con renglones para varios
    /// obreros hacen falta varias firmas: una firma amparando material que
    /// recibieron cinco personas distintas no sirve como evidencia de entrega de
    /// EPP, y ese respaldo es justamente lo que hace valioso al módulo.
    ///
    /// Es un acto de entrega, no un renglón por persona: cuando lo pedido llega
    /// en partes, la misma persona firma varias veces el mismo documento, una por
    /// cada vez que se lleva material. Cada renglón guarda cuál de esas firmas lo
    /// amparó.
    /// </summary>
    [Table("RequisicionesEntregas")]
    public class RequisicionEntrega
    {
        [Key]
        public int IdRequisicionEntrega { get; set; }

        [Required]
        public int IdRequisicion { get; set; }

        /// <summary>Quien recibe y firma.</summary>
        [Required]
        public int IdEmpleado { get; set; }

        [Required]
        public DateTime FechaEntrega { get; set; } = DateTime.Now;

        /// <summary>Almacenista que despachó.</summary>
        [Required]
        public string IdUsuarioEntrega { get; set; } = string.Empty;

        [Required]
        [MaxLength(500000)]
        public string FirmaDigital { get; set; } = string.Empty;

        [MaxLength(300)]
        public string? FotoEvidencia { get; set; }

        [MaxLength(500)]
        public string? Observaciones { get; set; }

        // ===== NAVEGACIÓN =====

        [ForeignKey("IdRequisicion")]
        public virtual RequisicionEPP? Requisicion { get; set; }

        [ForeignKey("IdEmpleado")]
        public virtual Empleado? Empleado { get; set; }

        [ForeignKey("IdUsuarioEntrega")]
        public virtual Usuario? UsuarioEntrega { get; set; }
    }
}
