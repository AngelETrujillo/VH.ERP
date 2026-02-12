using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VH.Services.Entities
{
    [Table("MaterialesEPP")]
    public class MaterialEPP
    {
        [Key]
        public int IdMaterial { get; set; }
        [Required]
        [MaxLength(200)]
        public string Nombre { get; set; } = string.Empty;
        [MaxLength(500)]
        public string Descripcion { get; set; } = string.Empty;
        [Required]
        public int IdUnidadMedida { get; set; }
        public decimal CostoUnitarioEstimado { get; set; }
        public int? VidaUtilDiasDefault { get; set; }
        public bool EsDesechable { get; set; } = false;
        public CategoriaRiesgoMaterial CategoriaRiesgo { get; set; } = CategoriaRiesgoMaterial.Medio;
        public bool Activo { get; set; } = true;

        // ===== PROPIEDADES DE NAVEGACIÓN =====

        [ForeignKey("IdUnidadMedida")]
        public virtual UnidadMedida? UnidadMedida { get; set; }
        public virtual ICollection<CompraEPP> Compras { get; set; } = new List<CompraEPP>();
        public virtual ICollection<Inventario> Inventarios { get; set; } = new List<Inventario>();
        public virtual ConfiguracionMaterialEPP? Configuracion { get; set; }
        public virtual ICollection<AlertaConsumo> Alertas { get; set; } = new List<AlertaConsumo>();
    }

    public enum CategoriaRiesgoMaterial
    {
        Bajo = 1,
        Medio = 2,
        Alto = 3
    }
}