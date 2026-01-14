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

        // Claves Foráneas
        [Required]
        public int IdEmpleado { get; set; }

        [Required]
        public int IdMaterial { get; set; }

        [Required]
        public int IdProveedor { get; set; }

        // Campos de la Transacción
        [Required]
        public DateTime FechaEntrega { get; set; }

        [Required]
        public decimal CantidadEntregada { get; set; }

        [MaxLength(20)]
        public string TallaEntregada { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Observaciones { get; set; } = string.Empty;

        // Navigation Properties
        [ForeignKey("IdEmpleado")]
        public virtual Empleado? Empleado { get; set; }

        [ForeignKey("IdMaterial")]
        public virtual MaterialEPP? MaterialEPP { get; set; }

        [ForeignKey("IdProveedor")]
        public virtual Proveedor? Proveedor { get; set; }
    }
}