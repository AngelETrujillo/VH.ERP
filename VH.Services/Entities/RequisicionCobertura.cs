using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VH.Services.Entities
{
    /// <summary>
    /// De dónde va a salir lo que pide un renglón de requisición: de la existencia
    /// que se le apartó, o de un renglón concreto de una orden de compra.
    ///
    /// Es tabla puente con cantidad, no un simple campo, porque la cobertura puede
    /// ser mixta y parcial: si se piden diez cascos, hay cuatro en el almacén y se
    /// compran seis, ese renglón queda cubierto por dos orígenes distintos. Con
    /// consumibles, que se piden por volumen, eso es lo normal.
    ///
    /// Es también lo que impide comprar dos veces lo mismo: la bandeja de faltantes
    /// sólo mira renglones sin cobertura.
    /// </summary>
    [Table("RequisicionesCobertura")]
    public class RequisicionCobertura
    {
        [Key]
        public int IdRequisicionCobertura { get; set; }

        [Required]
        public int IdRequisicionDetalle { get; set; }

        [Required]
        public OrigenCobertura Origen { get; set; }

        /// <summary>Renglón de la orden que lo cubre. Nulo si sale de existencia.</summary>
        public int? IdOrdenCompraDetalle { get; set; }

        [Required]
        public decimal Cantidad { get; set; }

        // ===== NAVEGACIÓN =====

        [ForeignKey("IdRequisicionDetalle")]
        public virtual RequisicionEPPDetalle? RequisicionDetalle { get; set; }

        [ForeignKey("IdOrdenCompraDetalle")]
        public virtual OrdenCompraDetalle? OrdenCompraDetalle { get; set; }
    }

    public enum OrigenCobertura
    {
        /// <summary>Sale del material que ya estaba en el almacén.</summary>
        Existencia = 0,

        /// <summary>Sale de lo que se pidió al proveedor.</summary>
        OrdenCompra = 1
    }
}
