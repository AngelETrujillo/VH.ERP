using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VH.Services.Entities
{
    /// <summary>
    /// Catálogo de materiales de Equipo de Protección Personal.
    /// El precio unitario estimado se actualiza con el último precio de compra.
    /// </summary>
    [Table("MaterialesEPP")]
    public class MaterialEPP
    {
        [Key]
        public int IdMaterial { get; set; }

        /// <summary>
        /// Nombre del material (ej: "Casco de Seguridad", "Guantes de Nitrilo")
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Descripción detallada del material
        /// </summary>
        [MaxLength(500)]
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>
        /// Unidad de medida del material (FK)
        /// </summary>
        [Required]
        public int IdUnidadMedida { get; set; }

        /// <summary>
        /// Último precio de compra registrado.
        /// Se actualiza automáticamente cada vez que se registra una compra.
        /// </summary>
        public decimal CostoUnitarioEstimado { get; set; }

        /// <summary>
        /// Indica si el material está activo en el catálogo
        /// </summary>
        public bool Activo { get; set; } = true;

        // ===== PROPIEDADES DE NAVEGACIÓN =====

        [ForeignKey("IdUnidadMedida")]
        public virtual UnidadMedida? UnidadMedida { get; set; }

        /// <summary>
        /// Historial de compras de este material (para análisis de precios por proveedor)
        /// </summary>
        public virtual ICollection<CompraEPP> Compras { get; set; } = new List<CompraEPP>();

        /// <summary>
        /// Registros de inventario en diferentes almacenes
        /// </summary>
        public virtual ICollection<Inventario> Inventarios { get; set; } = new List<Inventario>();
    }
}