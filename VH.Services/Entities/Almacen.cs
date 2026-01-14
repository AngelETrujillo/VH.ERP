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
        public string Nombre { get; set; } = string.Empty; // Ej: "Almacén Central Obra A"

        public string Descripcion { get; set; } = string.Empty;
        public string Domicilio { get; set; } = string.Empty;

        [MaxLength(50)]
        public string TipoUbicacion { get; set; } = string.Empty; // Ej: "Contenedor", "Bodega", "Patio"

        public bool Activo { get; set; } = true;

        // Relación con Proyecto
        public int IdProyecto { get; set; }

        [ForeignKey("IdProyecto")]
        public virtual Proyecto? Proyecto { get; set; }

        // Relación con Inventario (Un almacén tiene muchos registros de inventario)
        public virtual ICollection<Inventario>? Inventarios { get; set; }
    }
}