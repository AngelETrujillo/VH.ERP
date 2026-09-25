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

        // La vida útil vive en Material.VidaUtilDiasDefault, que es donde la lee
        // todo el sistema. Tenerla también aquí daba dos respuestas posibles a la
        // misma pregunta, sin nada que las obligara a coincidir.

        [Required]
        public int FrecuenciaMinimaDias { get; set; }
        public decimal? CantidadMaximaMensual { get; set; }
        public decimal? CantidadMaximaPorEntrega { get; set; }
        // Lo mismo con "vuelve o no vuelve": manda Material.EsRetornable, que es
        // el que lee Devoluciones. RequiereDevolucion sólo se mostraba en la
        // pantalla de alertas y nadie más la consultaba: una bandera muerta que
        // podía contradecir a la viva.
        public int UmbralAlertaPorcentaje { get; set; } = 70;
        public bool Activo { get; set; } = true;

        // ===== PROPIEDADES DE NAVEGACIÓN =====

        [ForeignKey("IdMaterial")]
        public virtual Material? Material { get; set; }
    }
}