using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VH.Services.Entities
{
    [Table("MaterialesEPP")]
    public class MaterialEPP
    {
        [Key]
        public int IdMaterial { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;

        // CAMBIO: Ahora usamos ID
        public int IdUnidadMedida { get; set; }

        [ForeignKey("IdUnidadMedida")]
        public virtual UnidadMedida? UnidadMedida { get; set; }

        public decimal CostoUnitarioEstimado { get; set; }
        public bool Activo { get; set; }

        // Relación inversa para saber en qué almacenes está este material
        public virtual ICollection<Inventario>? Inventarios { get; set; }
    }
}