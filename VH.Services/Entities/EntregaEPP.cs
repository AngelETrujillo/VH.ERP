using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VH.Services.Entities
{
    /// <summary>
    /// Representa una entrega de material EPP a un empleado.
    /// Ahora está vinculada a un lote/compra específico para saber
    /// de dónde sale el material y descontar del inventario correcto.
    /// </summary>
    [Table("EntregasEPP")]
    public class EntregaEPP
    {
        [Key]
        public int IdEntrega { get; set; }

        // ===== RELACIONES (Claves Foráneas) =====

        /// <summary>
        /// Empleado que recibe el material
        /// </summary>
        [Required]
        public int IdEmpleado { get; set; }

        /// <summary>
        /// Lote/Compra de donde sale el material.
        /// Esto permite saber de qué proveedor vino y a qué precio se compró.
        /// </summary>
        [Required]
        public int IdCompra { get; set; }

        // ===== DATOS DE LA ENTREGA =====

        /// <summary>
        /// Fecha en que se entregó el material al empleado
        /// </summary>
        [Required]
        public DateTime FechaEntrega { get; set; }

        /// <summary>
        /// Cantidad de unidades entregadas
        /// </summary>
        [Required]
        public decimal CantidadEntregada { get; set; }

        /// <summary>
        /// Talla del equipo entregado (si aplica)
        /// </summary>
        [MaxLength(20)]
        public string TallaEntregada { get; set; } = string.Empty;

        /// <summary>
        /// Observaciones adicionales sobre la entrega
        /// </summary>
        [MaxLength(500)]
        public string Observaciones { get; set; } = string.Empty;

        // ===== PROPIEDADES DE NAVEGACIÓN =====

        [ForeignKey("IdEmpleado")]
        public virtual Empleado? Empleado { get; set; }

        [ForeignKey("IdCompra")]
        public virtual CompraEPP? Compra { get; set; }
    }
}