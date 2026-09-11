using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VH.Services.Entities
{
    /// <summary>
    /// Renglón de una orden de compra: un material con destino a un almacén.
    ///
    /// La consolidación agrupa por material y almacén destino, no sólo por
    /// material: dos obras distintas reciben en bodegas distintas, y el desglose
    /// es lo que permite recibir y surtir cada una por su lado.
    /// </summary>
    [Table("OrdenesCompraDetalle")]
    public class OrdenCompraDetalle
    {
        [Key]
        public int IdOrdenCompraDetalle { get; set; }

        [Required]
        public int IdOrdenCompra { get; set; }

        [Required]
        public int IdMaterial { get; set; }

        /// <summary>Bodega a la que debe llegar este renglón.</summary>
        [Required]
        public int IdAlmacenDestino { get; set; }

        [Required]
        public decimal CantidadPedida { get; set; }

        /// <summary>Lo que ya llegó. Menor que lo pedido mientras la entrega sea parcial.</summary>
        public decimal CantidadRecibida { get; set; }

        [Required]
        public decimal PrecioUnitarioPactado { get; set; }

        [MaxLength(500)]
        public string? Observaciones { get; set; }

        // ===== NAVEGACIÓN =====

        [ForeignKey("IdOrdenCompra")]
        public virtual OrdenCompra? OrdenCompra { get; set; }

        [ForeignKey("IdMaterial")]
        public virtual Material? Material { get; set; }

        [ForeignKey("IdAlmacenDestino")]
        public virtual Almacen? AlmacenDestino { get; set; }

        /// <summary>Renglones de requisición que este pedido viene a cubrir.</summary>
        public virtual ICollection<RequisicionCobertura> Coberturas { get; set; } = new List<RequisicionCobertura>();

        // ===== CALCULADAS =====

        [NotMapped]
        public decimal Importe => CantidadPedida * PrecioUnitarioPactado;

        /// <summary>Lo que sigue viniendo en camino. Descuenta del faltante a comprar.</summary>
        [NotMapped]
        public decimal EnTransito => Math.Max(0, CantidadPedida - CantidadRecibida);

        [NotMapped]
        public bool Completo => CantidadRecibida >= CantidadPedida;
    }
}
