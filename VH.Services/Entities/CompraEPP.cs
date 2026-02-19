using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VH.Services.Entities
{
    [Table("ComprasEPP")]
    public class CompraEPP
    {
        [Key]
        public int IdCompra { get; set; }

        // ===== RELACIONES (Claves Foráneas) =====
        [Required]
        public int IdMaterial { get; set; }
        [Required]
        public int IdProveedor { get; set; }
        [Required]
        public int IdAlmacen { get; set; }

        // ===== DATOS DE LA COMPRA =====
        [Required]
        public DateTime FechaCompra { get; set; }
        [Required]
        public decimal CantidadComprada { get; set; }
        [Required]
        public decimal CantidadDisponible { get; set; }
        [Required]
        public decimal PrecioUnitario { get; set; }
        [MaxLength(50)]
        public string NumeroDocumento { get; set; } = string.Empty;
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
        [NotMapped]
        public decimal CostoTotal => CantidadComprada * PrecioUnitario;
        [NotMapped]
        public bool TieneDisponible => CantidadDisponible > 0;
    }
}