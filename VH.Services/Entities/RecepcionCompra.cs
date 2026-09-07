using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace VH.Services.Entities
{
    /// <summary>
    /// La llegada del material a una bodega concreta contra una orden de compra.
    ///
    /// Es la pieza que faltaba entre el pedido y la existencia. Hasta ahora la
    /// compra se capturaba ya "recibida" de un solo golpe, sin distinguir lo que
    /// se pidió de lo que llegó: no había forma de registrar una entrega parcial,
    /// ni un precio distinto al pactado, ni material rechazado por calidad.
    ///
    /// Una orden admite varias recepciones, y con obras separadas es obligatorio:
    /// el proveedor entrega en Torre el martes y en Carretera el jueves, y cada
    /// entrega es su propio evento contra su propio almacén.
    /// </summary>
    [Table("RecepcionesCompra")]
    public class RecepcionCompra
    {
        [Key]
        public int IdRecepcion { get; set; }

        [Required]
        [MaxLength(20)]
        public string Folio { get; set; } = string.Empty;

        [Required]
        public int IdOrdenCompra { get; set; }

        /// <summary>Bodega donde se descargó. Una recepción entra a un solo almacén.</summary>
        [Required]
        public int IdAlmacen { get; set; }

        [Required]
        public DateTime FechaRecepcion { get; set; } = DateTime.Now;

        /// <summary>Almacenista que recibió y contó.</summary>
        [Required]
        public string IdUsuarioRecibe { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? NumeroFactura { get; set; }

        [MaxLength(36)]
        public string? UuidCFDI { get; set; }

        [MaxLength(500)]
        public string? Observaciones { get; set; }

        /// <summary>
        /// Documento de compra que esta recepción generó. La recepción es el
        /// camino normal de entrada al almacén; la captura manual de compra queda
        /// como excepción para la compra directa sin orden.
        /// </summary>
        public int? IdCompra { get; set; }

        // ===== NAVEGACIÓN =====

        [ForeignKey("IdOrdenCompra")]
        public virtual OrdenCompra? OrdenCompra { get; set; }

        [ForeignKey("IdAlmacen")]
        public virtual Almacen? Almacen { get; set; }

        [ForeignKey("IdUsuarioRecibe")]
        public virtual Usuario? UsuarioRecibe { get; set; }

        [ForeignKey("IdCompra")]
        public virtual CompraEPP? Compra { get; set; }

        public virtual ICollection<RecepcionCompraDetalle> Detalles { get; set; } = new List<RecepcionCompraDetalle>();

        // ===== CALCULADAS =====

        [NotMapped]
        public int TotalRenglones => Detalles?.Count ?? 0;

        [NotMapped]
        public decimal TotalRecibido => Detalles?.Sum(d => d.CantidadRecibida) ?? 0;

        [NotMapped]
        public decimal TotalAceptado => Detalles?.Sum(d => d.CantidadAceptada) ?? 0;

        [NotMapped]
        public decimal TotalRechazado => Detalles?.Sum(d => d.CantidadRechazada) ?? 0;

        /// <summary>Lo que costó de verdad lo que entró al almacén.</summary>
        [NotMapped]
        public decimal Importe => Detalles?.Sum(d => d.CantidadAceptada * d.PrecioUnitarioReal) ?? 0;

        [NotMapped]
        public bool HuboRechazo => Detalles?.Any(d => d.CantidadRechazada > 0) ?? false;

        [NotMapped]
        public bool HuboDiferenciaPrecio => Detalles?.Any(d => d.DiferenciaPrecio != 0) ?? false;
    }
}
