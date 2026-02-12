using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VH.Services.Entities
{
    [Table("Empleados")]
    public class Empleado
    {
        [Key]
        public int IdEmpleado { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string ApellidoPaterno { get; set; } = string.Empty;

        [MaxLength(100)]
        public string ApellidoMaterno { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string NumeroNomina { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Puesto { get; set; } = string.Empty;

        public int? IdPuesto { get; set; }

        public DateTime? FechaIngreso { get; set; }

        public decimal PuntuacionRiesgoActual { get; set; } = 0;

        public DateTime? FechaUltimoCalculoRiesgo { get; set; }

        public bool Activo { get; set; } = true;

        // Clave Foránea
        [Required]
        public int IdProyecto { get; set; }

        // ===== PROPIEDADES DE NAVEGACIÓN =====

        [ForeignKey("IdProyecto")]
        public virtual Proyecto? Proyecto { get; set; }

        [ForeignKey("IdPuesto")]
        public virtual Puesto? PuestoCatalogo { get; set; }

        public virtual ICollection<EntregaEPP> EntregasEPP { get; set; } = new List<EntregaEPP>();

        public virtual ICollection<AlertaConsumo> Alertas { get; set; } = new List<AlertaConsumo>();

        public virtual ICollection<EstadisticaEmpleadoMensual> Estadisticas { get; set; } = new List<EstadisticaEmpleadoMensual>();

        // ===== PROPIEDADES CALCULADAS =====

        [NotMapped]
        public string NombreCompleto => $"{Nombre} {ApellidoPaterno} {ApellidoMaterno}".Trim();
    }
}