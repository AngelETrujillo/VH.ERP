using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VH.Services.Entities
{
    [Table("Almacenes")]
    public class Almacen
    {
        [Key]
        public int IdAlmacen { get; set; }
        [Required]
        [MaxLength(200)]
        public string Nombre { get; set; } = string.Empty;
        [MaxLength(500)]
        public string Descripcion { get; set; } = string.Empty;
        [MaxLength(300)]
        public string Domicilio { get; set; } = string.Empty;
        [MaxLength(50)]
        public string TipoUbicacion { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;
        [Required]
        public int IdProyecto { get; set; }

        // ===== PROPIEDADES DE NAVEGACIÓN =====

        [ForeignKey("IdProyecto")]
        public virtual Proyecto? Proyecto { get; set; }
        public virtual ICollection<Inventario> Inventarios { get; set; } = new List<Inventario>();
        public virtual ICollection<CompraEPP> Compras { get; set; } = new List<CompraEPP>();
    }
}