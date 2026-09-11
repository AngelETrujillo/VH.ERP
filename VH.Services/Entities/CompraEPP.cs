using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VH.Services.Entities
{
    /// <summary>
    /// Documento de compra: una factura o remisión de un proveedor.
    ///
    /// Antes esta tabla hacía tres trabajos a la vez —documento, renglón y lote—,
    /// de modo que una factura con tres materiales eran tres registros sueltos
    /// unidos sólo por el texto tecleado en NumeroDocumento. Ahora la cabecera es
    /// el documento y cada <see cref="CompraEPPDetalle"/> es un renglón, que es
    /// también el lote de inventario.
    /// </summary>
    [Table("ComprasEPP")]
    public class CompraEPP
    {
        [Key]
        public int IdCompra { get; set; }

        [Required]
        public int IdProveedor { get; set; }

        [Required]
        public DateTime FechaCompra { get; set; }

        [MaxLength(50)]
        public string NumeroDocumento { get; set; } = string.Empty;

        /// <summary>Folio fiscal del CFDI, para conciliar con contabilidad.</summary>
        [MaxLength(36)]
        public string? UuidCFDI { get; set; }

        // ===== IMPORTES DEL DOCUMENTO =====

        public decimal Subtotal { get; set; }
        public decimal Iva { get; set; }
        public decimal Total { get; set; }

        [MaxLength(3)]
        public string Moneda { get; set; } = "MXN";

        [MaxLength(500)]
        public string Observaciones { get; set; } = string.Empty;

        // ===== PROPIEDADES DE NAVEGACIÓN =====

        [ForeignKey("IdProveedor")]
        public virtual Proveedor? Proveedor { get; set; }

        public virtual ICollection<CompraEPPDetalle> Detalles { get; set; } = new List<CompraEPPDetalle>();

        // ===== PROPIEDADES CALCULADAS =====

        /// <summary>Suma de los renglones, antes de impuestos.</summary>
        [NotMapped]
        public decimal SubtotalCalculado => Detalles?.Sum(d => d.Cantidad * d.PrecioUnitario) ?? 0;

        [NotMapped]
        public int TotalRenglones => Detalles?.Count ?? 0;

        /// <summary>Queda algo por surtir en alguno de sus lotes.</summary>
        [NotMapped]
        public bool TieneDisponible => Detalles?.Any(d => d.CantidadDisponible > 0) ?? false;
    }
}
