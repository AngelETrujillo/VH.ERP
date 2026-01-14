using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VH.Services.Entities
{
    [Table("Proyectos")]
    public class Proyecto
    {
        [Key]
        public int IdProyecto { get; set; }

        [Required]
        [MaxLength(200)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(100)]
        public string TipoObra { get; set; } = string.Empty;

        [Required]
        public DateTime FechaInicio { get; set; }

        [Required]
        public decimal PresupuestoTotal { get; set; }

        // Navigation Properties - Relación 1:N
        public virtual ICollection<ConceptoPartida> ConceptosPartidas { get; set; } = new List<ConceptoPartida>();
        public virtual ICollection<Empleado> Empleados { get; set; } = new List<Empleado>();
        public virtual ICollection<Almacen> Almacenes { get; set; } = new List<Almacen>();
    }
}