using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VH.Services.Entities
{
    /// <summary>
    /// Representa un almacén físico donde se guardan los materiales EPP.
    /// Puede estar asociado a un proyecto/obra específica.
    /// </summary>
    [Table("Almacenes")]
    public class Almacen
    {
        [Key]
        public int IdAlmacen { get; set; }

        /// <summary>
        /// Nombre identificador del almacén (ej: "Almacén Central Obra Norte")
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Descripción adicional del almacén
        /// </summary>
        [MaxLength(500)]
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>
        /// Dirección física del almacén
        /// </summary>
        [MaxLength(300)]
        public string Domicilio { get; set; } = string.Empty;

        /// <summary>
        /// Tipo de ubicación (ej: "Contenedor", "Bodega", "Patio")
        /// </summary>
        [MaxLength(50)]
        public string TipoUbicacion { get; set; } = string.Empty;

        /// <summary>
        /// Indica si el almacén está activo
        /// </summary>
        public bool Activo { get; set; } = true;

        /// <summary>
        /// Proyecto/Obra al que pertenece este almacén
        /// </summary>
        [Required]
        public int IdProyecto { get; set; }

        // ===== PROPIEDADES DE NAVEGACIÓN =====

        [ForeignKey("IdProyecto")]
        public virtual Proyecto? Proyecto { get; set; }

        /// <summary>
        /// Registros de inventario en este almacén
        /// </summary>
        public virtual ICollection<Inventario> Inventarios { get; set; } = new List<Inventario>();

        /// <summary>
        /// Compras/Lotes de material almacenados aquí
        /// </summary>
        public virtual ICollection<CompraEPP> Compras { get; set; } = new List<CompraEPP>();
    }
}