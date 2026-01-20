using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VH.Services.Entities
{
    /// <summary>
    /// Catálogo de proveedores de materiales EPP.
    /// </summary>
    [Table("Proveedores")]
    public class Proveedor
    {
        [Key]
        public int IdProveedor { get; set; }

        /// <summary>
        /// Razón social o nombre del proveedor
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Registro Federal de Contribuyentes
        /// </summary>
        [Required]
        [MaxLength(13)]
        public string RFC { get; set; } = string.Empty;

        /// <summary>
        /// Nombre de la persona de contacto
        /// </summary>
        [MaxLength(200)]
        public string Contacto { get; set; } = string.Empty;

        /// <summary>
        /// Teléfono de contacto
        /// </summary>
        [MaxLength(20)]
        public string Telefono { get; set; } = string.Empty;

        /// <summary>
        /// Indica si el proveedor está activo
        /// </summary>
        public bool Activo { get; set; } = true;

        // ===== PROPIEDADES DE NAVEGACIÓN =====

        /// <summary>
        /// Historial de compras realizadas a este proveedor (para análisis y negociación)
        /// </summary>
        public virtual ICollection<CompraEPP> Compras { get; set; } = new List<CompraEPP>();
    }
}