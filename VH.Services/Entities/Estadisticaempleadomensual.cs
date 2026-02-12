using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VH.Services.Entities
{
    /// <summary>
    /// Estadísticas mensuales agregadas por empleado.
    /// Facilita consultas rápidas para rankings y dashboards sin recalcular cada vez.
    /// </summary>
    [Table("EstadisticasEmpleadoMensual")]
    public class EstadisticaEmpleadoMensual
    {
        [Key]
        public int IdEstadistica { get; set; }

        /// <summary>
        /// Empleado al que corresponden las estadísticas
        /// </summary>
        [Required]
        public int IdEmpleado { get; set; }

        /// <summary>
        /// Proyecto donde estaba asignado el empleado
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
        /// Total de entregas recibidas en el período
        /// </summary>
        public int TotalEntregas { get; set; }

        /// <summary>
        /// Suma total de unidades recibidas
        /// </summary>
        public decimal TotalUnidades { get; set; }

        /// <summary>
        /// Costo total del material entregado
        /// </summary>
        public decimal CostoTotal { get; set; }

        /// <summary>
        /// Número de materiales distintos solicitados
        /// </summary>
        public int MaterialesDistintos { get; set; }

        /// <summary>
        /// Promedio de desviación respecto a vida útil esperada (%)
        /// Valores negativos indican solicitudes prematuras
        /// </summary>
        public decimal PromedioDesviacionVidaUtil { get; set; }

        /// <summary>
        /// Número de alertas generadas en el período
        /// </summary>
        public int AlertasGeneradas { get; set; }

        /// <summary>
        /// Puntuación de riesgo calculada (0-100)
        /// Mayor puntuación = mayor riesgo de anomalía
        /// </summary>
        public decimal PuntuacionRiesgo { get; set; }

        /// <summary>
        /// Fecha de última actualización de estas estadísticas
        /// </summary>
        public DateTime FechaActualizacion { get; set; } = DateTime.Now;

        // ===== PROPIEDADES DE NAVEGACIÓN =====

        [ForeignKey("IdEmpleado")]
        public virtual Empleado? Empleado { get; set; }

        [ForeignKey("IdProyecto")]
        public virtual Proyecto? Proyecto { get; set; }
    }
}