using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VH.Services.Entities
{
    [Table("Modulos")]
    public class Modulo
    {
        [Key]
        public int IdModulo { get; set; }

        [Required]
        [MaxLength(50)]
        public string Codigo { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Descripcion { get; set; }

        [MaxLength(50)]
        public string? Icono { get; set; }

        [MaxLength(100)]
        public string? ControllerName { get; set; }

        public int Orden { get; set; } = 0;

        public bool Activo { get; set; } = true;

        public int? IdModuloPadre { get; set; }

        [ForeignKey("IdModuloPadre")]
        public virtual Modulo? ModuloPadre { get; set; }

        public virtual ICollection<Modulo> SubModulos { get; set; } = new List<Modulo>();

        public virtual ICollection<RolPermiso> RolPermisos { get; set; } = new List<RolPermiso>();
    }
}