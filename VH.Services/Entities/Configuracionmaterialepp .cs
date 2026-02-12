using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VH.Services.Entities
{
    /// <summary>
    /// Configuración de parámetros de control para cada material EPP.
    /// Define vida útil, frecuencias mínimas y cantidades máximas para detectar anomalías.
    /// </summary>
    [Table("ConfiguracionesMaterialEPP")]
    public class ConfiguracionMaterialEPP
    {
        [Key]
        public int IdConfiguracion { get; set; }

        /// <summary>
        /// Material al que aplica esta configuración
        /// </summary>
        [Required]
        public int IdMaterial { get; set; }

        /// <summary>
        /// Vida útil esperada en días (ej: casco = 365, guantes = 30)
        /// Si se solicita antes de este tiempo, genera alerta
        /// </summary>
        [Required]
        public int VidaUtilDias { get; set; }

        /// <summary>
        /// Días mínimos que deben pasar entre solicitudes del mismo material
        /// </summary>
        [Required]
        public int FrecuenciaMinimaDias { get; set; }

        /// <summary>
        /// Cantidad máxima que un empleado puede solicitar por mes
        /// </summary>
        public decimal? CantidadMaximaMensual { get; set; }

        /// <summary>
        /// Cantidad máxima permitida en una sola entrega
        /// </summary>
        public decimal? CantidadMaximaPorEntrega { get; set; }

        /// <summary>
        /// Indica si se requiere devolver el material dañado para recibir uno nuevo
        /// </summary>
        public bool RequiereDevolucion { get; set; } = false;

        /// <summary>
        /// Porcentaje de tolerancia sobre la vida útil (default 70%)
        /// Si solicita antes del (VidaUtilDias * UmbralAlertaPorcentaje/100), genera alerta
        /// </summary>
        public int UmbralAlertaPorcentaje { get; set; } = 70;

        /// <summary>
        /// Indica si esta configuración está activa
        /// </summary>
        public bool Activo { get; set; } = true;

        // ===== PROPIEDADES DE NAVEGACIÓN =====

        [ForeignKey("IdMaterial")]
        public virtual MaterialEPP? Material { get; set; }
    }
}