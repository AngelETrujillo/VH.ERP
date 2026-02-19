using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VH.Services.Entities
{
    [Table("EntregasEPP")]
    public class EntregaEPP
    {
        [Key]
        public int IdEntrega { get; set; }

        // ===== RELACIONES (Claves Foráneas) =====
        [Required]
        public int IdEmpleado { get; set; }
        [Required]
        public int IdCompra { get; set; }

        // ===== DATOS DE LA ENTREGA =====
        [Required]
        public DateTime FechaEntrega { get; set; }
        [Required]
        public decimal CantidadEntregada { get; set; }
        [MaxLength(20)]
        public string TallaEntregada { get; set; } = string.Empty;
        [MaxLength(500)]
        public string Observaciones { get; set; } = string.Empty;

        // ===== PROPIEDADES DE NAVEGACIÓN =====

        [ForeignKey("IdEmpleado")]
        public virtual Empleado? Empleado { get; set; }

        [ForeignKey("IdCompra")]
        public virtual CompraEPP? Compra { get; set; }
    }
}