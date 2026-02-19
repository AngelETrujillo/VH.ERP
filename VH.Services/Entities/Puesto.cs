using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VH.Services.Entities
{
    [Table("Puestos")]
    public class Puesto
    {
        [Key]
        public int IdPuesto { get; set; }
        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;
        [MaxLength(500)]
        public string Descripcion { get; set; } = string.Empty;
        public NivelRiesgoEPP NivelRiesgoEPP { get; set; } = NivelRiesgoEPP.Medio;
        public bool Activo { get; set; } = true;

        // ===== PROPIEDADES DE NAVEGACIÓN =====

        public virtual ICollection<Empleado> Empleados { get; set; } = new List<Empleado>();
    }
    public enum NivelRiesgoEPP
    {
        Bajo = 1,
        Medio = 2,
        Alto = 3
    }
}