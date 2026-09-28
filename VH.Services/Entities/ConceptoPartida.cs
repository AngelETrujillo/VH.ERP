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

        /// <summary>
        /// Costo de material presupuestado por unidad de obra: lo que se estima
        /// gastar en material por cada castillo, por cada metro de junta.
        ///
        /// Se guarda el precio y no el total porque el total se sigue de él: si
        /// mañana cambia la cantidad estimada, un importe capturado a mano se
        /// quedaría describiendo un alcance que ya no es.
        ///
        /// Opcional: las partidas que se dieron de alta antes de que esto
        /// existiera no tienen precio, y una partida sin presupuestar todavía es
        /// una partida válida. Sin él no hay contra qué comparar y la pantalla
        /// lo dice en vez de inventar un cero.
        /// </summary>
        public decimal? PrecioUnitarioEstimado { get; set; }

        /// <summary>Lo presupuestado para la partida. Nulo mientras no tenga precio.</summary>
        [NotMapped]
        public decimal? CostoTotalEstimado =>
            PrecioUnitarioEstimado.HasValue
                ? PrecioUnitarioEstimado.Value * CantidadEstimada
                : null;

        // Navigation Properties
        [ForeignKey("IdProyecto")]
        public virtual Proyecto? Proyecto { get; set; }

        [ForeignKey("IdUnidadMedida")]
        public virtual UnidadMedida? UnidadMedida { get; set; }
    }
}