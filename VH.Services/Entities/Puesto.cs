using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VH.Services.Entities
{
    /// <summary>
    /// Catálogo de puestos de trabajo.
    /// Permite agrupar empleados para comparar consumos entre roles similares.
    /// </summary>
    [Table("Puestos")]
    public class Puesto
    {
        [Key]
        public int IdPuesto { get; set; }

        /// <summary>
        /// Nombre del puesto (ej: "Albañil", "Soldador", "Electricista")
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Descripción de las funciones del puesto
        /// </summary>
        [MaxLength(500)]
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>
        /// Nivel de riesgo del puesto para EPP (afecta umbrales de alerta)
        /// </summary>
        public NivelRiesgoEPP NivelRiesgoEPP { get; set; } = NivelRiesgoEPP.Medio;

        /// <summary>
        /// Indica si el puesto está activo
        /// </summary>
        public bool Activo { get; set; } = true;

        // ===== PROPIEDADES DE NAVEGACIÓN =====

        public virtual ICollection<Empleado> Empleados { get; set; } = new List<Empleado>();
    }

    /// <summary>
    /// Nivel de riesgo del puesto para consumo de EPP
    /// </summary>
    public enum NivelRiesgoEPP
    {
        Bajo = 1,
        Medio = 2,
        Alto = 3
    }
}