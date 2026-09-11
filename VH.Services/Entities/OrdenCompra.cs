using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace VH.Services.Entities
{
    /// <summary>
    /// Pedido a un proveedor, nacido de consolidar lo que falta para surtir
    /// requisiciones ya autorizadas.
    ///
    /// Es distinta de la compra: la orden es lo que se pidió y aún no llega; la
    /// compra es lo que ya entró al almacén. Entre una y otra está la recepción.
    /// </summary>
    [Table("OrdenesCompra")]
    public class OrdenCompra
    {
        [Key]
        public int IdOrdenCompra { get; set; }

        [Required]
        [MaxLength(20)]
        public string Folio { get; set; } = string.Empty;

        [Required]
        public int IdProveedor { get; set; }

        [Required]
        public DateTime FechaEmision { get; set; } = DateTime.Now;

        /// <summary>Cuándo se espera recibirla. Sirve para perseguir al proveedor.</summary>
        public DateTime? FechaEntregaEstimada { get; set; }

        /// <summary>Comprador que la emitió.</summary>
        [Required]
        public string IdUsuarioEmite { get; set; } = string.Empty;

        /// <summary>
        /// Estado del documento. Hoy nace emitida porque el Comprador la manda
        /// directo; el campo existe para poder intercalar una autorización por
        /// monto más adelante sin migrar nada.
        /// </summary>
        [Required]
        public EstadoOrdenCompra Estado { get; set; } = EstadoOrdenCompra.Emitida;

        [MaxLength(3)]
        public string Moneda { get; set; } = "MXN";

        [MaxLength(500)]
        public string? Observaciones { get; set; }

        [MaxLength(500)]
        public string? MotivoCancelacion { get; set; }

        // ===== NAVEGACIÓN =====

        [ForeignKey("IdProveedor")]
        public virtual Proveedor? Proveedor { get; set; }

        [ForeignKey("IdUsuarioEmite")]
        public virtual Usuario? UsuarioEmite { get; set; }

        public virtual ICollection<OrdenCompraDetalle> Detalles { get; set; } = new List<OrdenCompraDetalle>();

        // ===== CALCULADAS =====

        [NotMapped]
        public decimal Total => Detalles?.Sum(d => d.CantidadPedida * d.PrecioUnitarioPactado) ?? 0;

        [NotMapped]
        public int TotalRenglones => Detalles?.Count ?? 0;

        [NotMapped]
        public decimal TotalPiezas => Detalles?.Sum(d => d.CantidadPedida) ?? 0;

        /// <summary>Nada recibido todavía.</summary>
        [NotMapped]
        public bool SinRecibir => Detalles?.All(d => d.CantidadRecibida == 0) ?? true;

        /// <summary>Todo lo pedido ya llegó.</summary>
        [NotMapped]
        public bool RecibidaCompleta => Detalles?.All(d => d.CantidadRecibida >= d.CantidadPedida) ?? false;
    }

    public enum EstadoOrdenCompra
    {
        /// <summary>Enviada al proveedor, esperando material.</summary>
        Emitida = 0,

        /// <summary>Llegó parte de lo pedido.</summary>
        ParcialmenteRecibida = 1,

        /// <summary>Llegó todo.</summary>
        Recibida = 2,

        /// <summary>Se dio de baja; lo que cubría vuelve a la bandeja de faltantes.</summary>
        Cancelada = 3
    }
}
