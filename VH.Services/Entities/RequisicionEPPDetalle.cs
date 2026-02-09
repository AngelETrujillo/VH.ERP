using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VH.Services.Entities
{
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

        // ===== ENTREGA =====

        public int? IdCompra { get; set; }

        public decimal? CantidadEntregada { get; set; }

        // ===== NAVEGACIÓN =====

        [ForeignKey("IdRequisicion")]
        public virtual RequisicionEPP? Requisicion { get; set; }

        [ForeignKey("IdMaterial")]
        public virtual MaterialEPP? Material { get; set; }

        [ForeignKey("IdCompra")]
        public virtual CompraEPP? Compra { get; set; }
    }
}