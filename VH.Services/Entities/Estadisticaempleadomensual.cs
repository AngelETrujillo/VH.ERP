using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VH.Services.Entities
{
    [Table("EstadisticasEmpleadoMensual")]
    public class EstadisticaEmpleadoMensual
    {
        [Key]
        public int IdEstadistica { get; set; }
        [Required]
        public int IdEmpleado { get; set; }
        [Required]
        public int IdProyecto { get; set; }
        [Required]
        public int Anio { get; set; }
        [Required]
        public int Mes { get; set; }
        public int TotalEntregas { get; set; }
        public decimal TotalUnidades { get; set; }
        public decimal CostoTotal { get; set; }
        public int MaterialesDistintos { get; set; }
        public decimal PromedioDesviacionVidaUtil { get; set; }
        public int AlertasGeneradas { get; set; }
        public decimal PuntuacionRiesgo { get; set; }
        public DateTime FechaActualizacion { get; set; } = DateTime.Now;

        // ===== PROPIEDADES DE NAVEGACIÓN =====

        [ForeignKey("IdEmpleado")]
        public virtual Empleado? Empleado { get; set; }

        [ForeignKey("IdProyecto")]
        public virtual Proyecto? Proyecto { get; set; }
    }
}