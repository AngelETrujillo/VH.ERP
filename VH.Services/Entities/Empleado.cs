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

        public bool Activo { get; set; } = true;

        // Clave Foránea
        [Required]
        public int IdProyecto { get; set; }

        // Navigation Properties
        [ForeignKey("IdProyecto")]
        public virtual Proyecto? Proyecto { get; set; }

        public virtual ICollection<EntregaEPP> EntregasEPP { get; set; } = new List<EntregaEPP>();
    }
}