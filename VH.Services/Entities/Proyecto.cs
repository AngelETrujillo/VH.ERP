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
        public decimal? PresupuestoEPPMensual { get; set; }
        public int? EmpleadosEsperados { get; set; }
        public DateTime? FechaFinEstimada { get; set; }
        public bool Activo { get; set; } = true;

        // ===== PROPIEDADES DE NAVEGACIÓN =====

        public virtual ICollection<ConceptoPartida> ConceptosPartidas { get; set; } = new List<ConceptoPartida>();
        public virtual ICollection<Empleado> Empleados { get; set; } = new List<Empleado>();
        public virtual ICollection<Almacen> Almacenes { get; set; } = new List<Almacen>();
        public virtual ICollection<AlertaConsumo> Alertas { get; set; } = new List<AlertaConsumo>();
        public virtual ICollection<EstadisticaProyectoMensual> Estadisticas { get; set; } = new List<EstadisticaProyectoMensual>();
    }
}