using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VH.Services.Entities
{
    /// <summary>
    /// Representa una compra/lote de material EPP.
    /// Cada registro es una transacción de compra con un proveedor específico,
    /// permitiendo rastrear precios históricos y de qué lote salen las entregas.
    /// </summary>
    [Table("ComprasEPP")]
    public class CompraEPP
    {
        [Key]
        public int IdCompra { get; set; }

        // ===== RELACIONES (Claves Foráneas) =====

        /// <summary>
        /// Material que se compró
        /// </summary>
        [Required]
        public int IdMaterial { get; set; }

        /// <summary>
        /// Proveedor al que se le compró
        /// </summary>
        [Required]
        public int IdProveedor { get; set; }

        /// <summary>
        /// Almacén donde se guardó el material
        /// </summary>
        [Required]
        public int IdAlmacen { get; set; }

        // ===== DATOS DE LA COMPRA =====

        /// <summary>
        /// Fecha en que se realizó la compra
        /// </summary>
        [Required]
        public DateTime FechaCompra { get; set; }

        /// <summary>
        /// Cantidad de unidades compradas en este lote
        /// </summary>
        [Required]
        public decimal CantidadComprada { get; set; }

        /// <summary>
        /// Cantidad que aún está disponible de este lote (no entregada)
        /// Se reduce cada vez que se hace una entrega de este lote
        /// </summary>
        [Required]
        public decimal CantidadDisponible { get; set; }

        /// <summary>
        /// Precio unitario en el momento de la compra
        /// Permite análisis histórico de precios por proveedor
        /// </summary>
        [Required]
        public decimal PrecioUnitario { get; set; }

        /// <summary>
        /// Número de factura o documento de compra (opcional)
        /// </summary>
        [MaxLength(50)]
        public string NumeroDocumento { get; set; } = string.Empty;

        /// <summary>
        /// Observaciones adicionales sobre la compra
        /// </summary>
        [MaxLength(500)]
        public string Observaciones { get; set; } = string.Empty;

        // ===== PROPIEDADES DE NAVEGACIÓN =====

        [ForeignKey("IdMaterial")]
        public virtual MaterialEPP? Material { get; set; }

        [ForeignKey("IdProveedor")]
        public virtual Proveedor? Proveedor { get; set; }

        [ForeignKey("IdAlmacen")]
        public virtual Almacen? Almacen { get; set; }

        // ===== PROPIEDADES CALCULADAS =====

        /// <summary>
        /// Costo total de esta compra
        /// </summary>
        [NotMapped]
        public decimal CostoTotal => CantidadComprada * PrecioUnitario;

        /// <summary>
        /// Indica si aún hay unidades disponibles de este lote
        /// </summary>
        [NotMapped]
        public bool TieneDisponible => CantidadDisponible > 0;
    }
}