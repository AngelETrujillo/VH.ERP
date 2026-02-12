using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VH.Services.Entities
{
    /// <summary>
    /// Estadísticas mensuales agregadas por proyecto/obra.
    /// Permite análisis de consumo por frente de trabajo y desviaciones presupuestales.
    /// </summary>
    [Table("EstadisticasProyectoMensual")]
    public class EstadisticaProyectoMensual
    {
        [Key]
        public int IdEstadistica { get; set; }

        /// <summary>
        /// Proyecto al que corresponden las estadísticas
        /// </summary>
        [Required]
        public int IdProyecto { get; set; }

        /// <summary>
        /// Año del período
        /// </summary>
        [Required]
        public int Anio { get; set; }

        /// <summary>
        /// Mes del período (1-12)
        /// </summary>
        [Required]
        public int Mes { get; set; }

        /// <summary>
        /// Total de empleados activos en el proyecto durante el período
        /// </summary>
        public int TotalEmpleados { get; set; }

        /// <summary>
        /// Total de entregas realizadas en el proyecto
        /// </summary>
        public int TotalEntregas { get; set; }

        /// <summary>
        /// Total de unidades entregadas
        /// </summary>
        public decimal TotalUnidades { get; set; }

        /// <summary>
        /// Costo total del material EPP consumido
        /// </summary>
        public decimal CostoTotal { get; set; }

        /// <summary>
        /// Costo promedio por empleado en el período
        /// </summary>
        public decimal CostoPromedioPorEmpleado { get; set; }

        /// <summary>
        /// Presupuesto EPP asignado al proyecto para el mes
        /// </summary>
        public decimal PresupuestoAsignado { get; set; }

        /// <summary>
        /// Porcentaje de desviación vs presupuesto
        /// Positivo = sobre presupuesto, Negativo = bajo presupuesto
        /// </summary>
        public decimal DesviacionPresupuesto { get; set; }

        /// <summary>
        /// Número de alertas críticas generadas en el proyecto
        /// </summary>
        public int AlertasCriticas { get; set; }

        /// <summary>
        /// Total de alertas de cualquier severidad
        /// </summary>
        public int TotalAlertas { get; set; }

        /// <summary>
        /// Fecha de última actualización de estas estadísticas
        /// </summary>
        public DateTime FechaActualizacion { get; set; } = DateTime.Now;

        // ===== PROPIEDADES DE NAVEGACIÓN =====

        [ForeignKey("IdProyecto")]
        public virtual Proyecto? Proyecto { get; set; }
    }
}