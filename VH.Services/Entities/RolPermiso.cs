using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VH.Services.Entities
{
    [Table("RolPermisos")]
    public class RolPermiso
    {
        [Key]
        public int IdRolPermiso { get; set; }

        [Required]
        public string IdRol { get; set; } = string.Empty;

        [Required]
        public int IdModulo { get; set; }

        public bool PuedeVer { get; set; } = false;

        public bool PuedeCrear { get; set; } = false;

        public bool PuedeEditar { get; set; } = false;

        public bool PuedeEliminar { get; set; } = false;

        [ForeignKey("IdRol")]
        public virtual Rol? Rol { get; set; }

        [ForeignKey("IdModulo")]
        public virtual Modulo? Modulo { get; set; }
    }
}