using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VH.Services.Entities
{
    /// <summary>
    /// Un material contado en la puerta del almacén, contra el renglón de la orden
    /// que lo pidió.
    ///
    /// Lo recibido y lo aceptado se guardan por separado: lo que llegó dañado se
    /// cuenta, se deja asentado y no entra al inventario, y el renglón de la orden
    /// sigue esperando esa parte. El precio pactado vive en la orden y el real
    /// aquí; el costo del lote es el real, porque es lo que se pagó.
    /// </summary>
    [Table("RecepcionesCompraDetalle")]
    public class RecepcionCompraDetalle
    {
        [Key]
        public int IdRecepcionDetalle { get; set; }

        [Required]
        public int IdRecepcion { get; set; }

        [Required]
        public int IdOrdenCompraDetalle { get; set; }

        [Required]
        public int IdMaterial { get; set; }

        /// <summary>Lo que bajó del camión.</summary>
        [Required]
        public decimal CantidadRecibida { get; set; }

        /// <summary>
        /// Lo que se quedó. Menor que lo recibido cuando algo se rechaza por
        /// calidad; sólo esto entra al almacén y genera lote.
        /// </summary>
        [Required]
        public decimal CantidadAceptada { get; set; }

        /// <summary>Lo que se pagó de verdad, que manda sobre lo pactado.</summary>
        [Required]
        public decimal PrecioUnitarioReal { get; set; }

        [MaxLength(20)]
        public string? Talla { get; set; }

        public DateTime? FechaCaducidad { get; set; }

        /// <summary>Lote del proveedor, para rastrear un defecto hasta su origen.</summary>
        [MaxLength(50)]
        public string? LoteProveedor { get; set; }

        [MaxLength(500)]
        public string? MotivoRechazo { get; set; }

        /// <summary>Lote de inventario que esta línea generó.</summary>
        public int? IdCompraDetalle { get; set; }

        // ===== NAVEGACIÓN =====

        [ForeignKey("IdRecepcion")]
        public virtual RecepcionCompra? Recepcion { get; set; }

        [ForeignKey("IdOrdenCompraDetalle")]
        public virtual OrdenCompraDetalle? OrdenCompraDetalle { get; set; }

        [ForeignKey("IdMaterial")]
        public virtual Material? Material { get; set; }

        [ForeignKey("IdCompraDetalle")]
        public virtual CompraEPPDetalle? CompraDetalle { get; set; }

        // ===== CALCULADAS =====

        [NotMapped]
        public decimal CantidadRechazada => CantidadRecibida - CantidadAceptada;

        [NotMapped]
        public bool TuvoRechazo => CantidadRechazada > 0;

        [NotMapped]
        public decimal Importe => CantidadAceptada * PrecioUnitarioReal;

        /// <summary>
        /// Cuánto se desvió del precio pactado. Positivo significa que el
        /// proveedor cobró de más.
        /// </summary>
        [NotMapped]
        public decimal DiferenciaPrecio =>
            OrdenCompraDetalle == null ? 0 : PrecioUnitarioReal - OrdenCompraDetalle.PrecioUnitarioPactado;
    }
}
