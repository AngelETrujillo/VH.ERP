using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VH.Services.Entities
{
    [Table("EntregasEPP")]
    public class EntregaEPP
    {
        [Key]
        public int IdEntrega { get; set; }

        // ===== RELACIONES (Claves Foráneas) =====
        [Required]
        public int IdEmpleado { get; set; }

        /// <summary>Lote del que salió el material: un renglón de compra.</summary>
        [Required]
        public int IdCompraDetalle { get; set; }

        /// <summary>
        /// Acto de entrega que ampara esta salida, cuando vino de una requisición.
        ///
        /// Una firma puede amparar varias salidas: si lo pedido está repartido en
        /// dos lotes, se descuenta de cada uno por separado pero la persona firma
        /// una sola vez. Sin este enlace la ficha tenía que adivinar qué cubrió
        /// cada firma agrupando por trabajador, y con dos entregas al mismo obrero
        /// mostraba el acumulado del renglón en ambas.
        ///
        /// Nulo en las entregas sueltas, que no nacen de ninguna requisición.
        /// </summary>
        public int? IdRequisicionEntrega { get; set; }

        // ===== DATOS DE LA ENTREGA =====
        [Required]
        public DateTime FechaEntrega { get; set; }
        [Required]
        public decimal CantidadEntregada { get; set; }
        [MaxLength(20)]
        public string TallaEntregada { get; set; } = string.Empty;
        [MaxLength(500)]
        public string Observaciones { get; set; } = string.Empty;

        // ===== PROPIEDADES DE NAVEGACIÓN =====

        [ForeignKey("IdEmpleado")]
        public virtual Empleado? Empleado { get; set; }

        [ForeignKey("IdCompraDetalle")]
        public virtual CompraEPPDetalle? CompraDetalle { get; set; }

        [ForeignKey("IdRequisicionEntrega")]
        public virtual RequisicionEntrega? RequisicionEntrega { get; set; }
    }
}