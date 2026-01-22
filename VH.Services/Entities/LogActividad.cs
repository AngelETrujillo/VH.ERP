using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VH.Services.Entities
{
    [Table("LogsActividad")]
    public class LogActividad
    {
        [Key]
        public int IdLog { get; set; }

        [Required]
        public string IdUsuario { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Accion { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Entidad { get; set; }

        public int? IdEntidad { get; set; }

        [MaxLength(500)]
        public string? Descripcion { get; set; }

        [MaxLength(50)]
        public string? DireccionIP { get; set; }

        public DateTime Fecha { get; set; } = DateTime.UtcNow;

        // Navegación
        [ForeignKey("IdUsuario")]
        public virtual Usuario? Usuario { get; set; }
    }
}