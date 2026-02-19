using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VH.Services.Entities
{
    [Table("ConfiguracionesMaterialEPP")]
    public class ConfiguracionMaterialEPP
    {
        [Key]
        public int IdConfiguracion { get; set; }
        [Required]
        public int IdMaterial { get; set; }
        [Required]
        public int VidaUtilDias { get; set; }
        [Required]
        public int FrecuenciaMinimaDias { get; set; }
        public decimal? CantidadMaximaMensual { get; set; }
        public decimal? CantidadMaximaPorEntrega { get; set; }
        public bool RequiereDevolucion { get; set; } = false;
        public int UmbralAlertaPorcentaje { get; set; } = 70;
        public bool Activo { get; set; } = true;

        // ===== PROPIEDADES DE NAVEGACIÓN =====

        [ForeignKey("IdMaterial")]
        public virtual MaterialEPP? Material { get; set; }
    }
}