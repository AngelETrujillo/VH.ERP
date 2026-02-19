using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VH.Services.Entities
{
    [Table("EstadisticasProyectoMensual")]
    public class EstadisticaProyectoMensual
    {
        [Key]
        public int IdEstadistica { get; set; }
        [Required]
        public int IdProyecto { get; set; }
        [Required]
        public int Anio { get; set; }
        [Required]
        public int Mes { get; set; }
        public int TotalEmpleados { get; set; }
        public int TotalEntregas { get; set; }
        public decimal TotalUnidades { get; set; }
        public decimal CostoTotal { get; set; }
        public decimal CostoPromedioPorEmpleado { get; set; }
        public decimal PresupuestoAsignado { get; set; }
        public decimal DesviacionPresupuesto { get; set; }
        public int AlertasCriticas { get; set; }
        public int TotalAlertas { get; set; }
        public DateTime FechaActualizacion { get; set; } = DateTime.Now;

        // ===== PROPIEDADES DE NAVEGACIÓN =====

        [ForeignKey("IdProyecto")]
        public virtual Proyecto? Proyecto { get; set; }
    }
}