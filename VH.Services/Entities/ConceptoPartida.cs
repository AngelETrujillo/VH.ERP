using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VH.Services.Entities
{
    [Table("ConceptosPartidas")]
    public class ConceptoPartida
    {
        [Key]
        public int IdPartida { get; set; }

        [Required]
        public int IdProyecto { get; set; }

        [Required]
        [MaxLength(500)]
        public string Descripcion { get; set; } = string.Empty;

        [Required]
        public int IdUnidadMedida { get; set; }

        [Required]
        public decimal CantidadEstimada { get; set; }

        // Navigation Properties
        [ForeignKey("IdProyecto")]
        public virtual Proyecto? Proyecto { get; set; }

        [ForeignKey("IdUnidadMedida")]
        public virtual UnidadMedida? UnidadMedida { get; set; }
    }
}