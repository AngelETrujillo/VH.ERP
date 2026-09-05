using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VH.Services.Entities
{
    /// <summary>
    /// Renglón de una requisición: un material para un destino concreto.
    ///
    /// El renglón es la unidad de trabajo del circuito. Antes el empleado y el
    /// estado vivían en la cabecera, de modo que una requisición se cerraba entera
    /// aunque sólo se hubiera surtido uno de sus materiales, y no había forma de
    /// pedir para dos personas en un mismo documento.
    /// </summary>
    [Table("RequisicionesEPPDetalle")]
    public class RequisicionEPPDetalle
    {
        [Key]
        public int IdRequisicionDetalle { get; set; }

        [Required]
        public int IdRequisicion { get; set; }

        [Required]
        public int IdMaterial { get; set; }

        [Required]
        public decimal CantidadSolicitada { get; set; }

        [MaxLength(20)]
        public string? TallaSolicitada { get; set; }

        // ===== DESTINO =====

        /// <summary>
        /// Persona que recibe. Nulo cuando el renglón no va a nadie en concreto:
        /// un consumible se carga a la obra o a una partida, no a un obrero.
        /// </summary>
        public int? IdEmpleadoDestino { get; set; }

        /// <summary>Obra a la que se carga el consumo cuando no hay empleado.</summary>
        public int? IdProyectoDestino { get; set; }

        /// <summary>Partida presupuestal a la que se imputa, si aplica.</summary>
        public int? IdConceptoPartida { get; set; }

        // ===== ESTADO =====

        [Required]
        public EstadoRenglonRequisicion EstadoRenglon { get; set; } = EstadoRenglonRequisicion.Solicitado;

        [MaxLength(500)]
        public string? MotivoRechazo { get; set; }

        // ===== ENTREGA =====

        /// <summary>Lote del que se surtió el renglón.</summary>
        public int? IdCompraDetalle { get; set; }

        public decimal? CantidadEntregada { get; set; }

        // ===== NAVEGACIÓN =====

        [ForeignKey("IdRequisicion")]
        public virtual RequisicionEPP? Requisicion { get; set; }

        [ForeignKey("IdMaterial")]
        public virtual Material? Material { get; set; }

        [ForeignKey("IdCompraDetalle")]
        public virtual CompraEPPDetalle? CompraDetalle { get; set; }

        [ForeignKey("IdEmpleadoDestino")]
        public virtual Empleado? EmpleadoDestino { get; set; }

        [ForeignKey("IdProyectoDestino")]
        public virtual Proyecto? ProyectoDestino { get; set; }

        [ForeignKey("IdConceptoPartida")]
        public virtual ConceptoPartida? ConceptoPartida { get; set; }

        // ===== PROPIEDADES CALCULADAS =====

        /// <summary>Lo entregado a una persona necesita su firma; lo cargado a obra no.</summary>
        [NotMapped]
        public bool RequiereFirma => IdEmpleadoDestino.HasValue;

        [NotMapped]
        public bool EstaSurtido => EstadoRenglon == EstadoRenglonRequisicion.Surtido;

        /// <summary>Sigue vivo en el circuito: ni rechazado ni cancelado ni ya surtido.</summary>
        [NotMapped]
        public bool EstaPendiente =>
            EstadoRenglon != EstadoRenglonRequisicion.Rechazado &&
            EstadoRenglon != EstadoRenglonRequisicion.Cancelado &&
            EstadoRenglon != EstadoRenglonRequisicion.Surtido;
    }

    /// <summary>
    /// Ciclo de vida de un renglón. Los estados intermedios entre Autorizado y
    /// Surtido (reserva, compra, recepción) llegan con las fases siguientes; por
    /// ahora un renglón autorizado se surte directo del almacén.
    /// </summary>
    public enum EstadoRenglonRequisicion
    {
        Solicitado = 0,
        Autorizado = 1,
        Rechazado = 2,
        Surtido = 3,
        Cancelado = 4
    }
}
