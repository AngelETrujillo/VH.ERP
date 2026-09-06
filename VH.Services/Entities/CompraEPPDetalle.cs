using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VH.Services.Entities
{
    /// <summary>
    /// Renglón de una compra, que es también el lote de inventario del que salen
    /// las entregas. De aquí se descuenta <see cref="CantidadDisponible"/> y sobre
    /// este renglón se apoya el costeo: el precio pagado en este lote es el costo
    /// de todo lo que se entregue de él.
    ///
    /// El almacén vive en el renglón, no en la cabecera: una compra centralizada
    /// puede repartirse entre obras distintas.
    /// </summary>
    [Table("ComprasEPPDetalle")]
    public class CompraEPPDetalle
    {
        [Key]
        public int IdCompraDetalle { get; set; }

        [Required]
        public int IdCompra { get; set; }

        [Required]
        public int IdMaterial { get; set; }

        [Required]
        public int IdAlmacen { get; set; }

        [Required]
        public decimal Cantidad { get; set; }

        /// <summary>Lo que queda del lote. Baja con cada entrega.</summary>
        [Required]
        public decimal CantidadDisponible { get; set; }

        [Required]
        public decimal PrecioUnitario { get; set; }

        [MaxLength(20)]
        public string? Talla { get; set; }

        /// <summary>Caducidad impresa del lote, cuando el material la controla.</summary>
        public DateTime? FechaCaducidad { get; set; }

        /// <summary>
        /// Control de concurrencia: dos almacenistas despachando del mismo lote a
        /// la vez lo sobrevenderían sin esto.
        /// </summary>
        [Timestamp]
        public byte[]? RowVersion { get; set; }

        // ===== PROPIEDADES DE NAVEGACIÓN =====

        [ForeignKey("IdCompra")]
        public virtual CompraEPP? Compra { get; set; }

        [ForeignKey("IdMaterial")]
        public virtual Material? Material { get; set; }

        [ForeignKey("IdAlmacen")]
        public virtual Almacen? Almacen { get; set; }

        // ===== PROPIEDADES CALCULADAS =====

        [NotMapped]
        public decimal CostoTotal => Cantidad * PrecioUnitario;

        [NotMapped]
        public decimal CantidadEntregada => Cantidad - CantidadDisponible;

        [NotMapped]
        public bool TieneDisponible => CantidadDisponible > 0;

        [NotMapped]
        public bool EstaCaducado => FechaCaducidad.HasValue && FechaCaducidad.Value.Date < DateTime.Today;
    }
}
