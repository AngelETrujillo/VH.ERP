using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VH.Services.Entities
{
    [Table("UnidadesMedida")]
    public class UnidadMedida
    {
        [Key]
        public int IdUnidadMedida { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty; // Ej: "Pieza", "Kilogramo"

        [MaxLength(20)]
        public string Abreviatura { get; set; } = string.Empty; // Ej: "Pza", "Kg"

        public string Descripcion { get; set; } = string.Empty;
    }
}