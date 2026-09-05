using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VH.Services.Entities
{
    /// <summary>
    /// Artículo del catálogo: equipo de protección, consumible o herramienta.
    ///
    /// Antes se llamaba MaterialEPP porque el sistema sólo manejaba equipo de
    /// protección. El catálogo es ahora común a los tres tipos, y <see cref="TipoMaterial"/>
    /// decide qué reglas le aplican: las alertas de consumo (vida útil, frecuencia,
    /// solicitud prematura) sólo tienen sentido sobre EPP.
    /// </summary>
    [Table("Materiales")]
    public class Material
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

        // ===== CLASIFICACIÓN =====

        /// <summary>Qué es el artículo. Determina qué reglas de negocio le aplican.</summary>
        [Required]
        public TipoMaterial TipoMaterial { get; set; } = TipoMaterial.EPP;

        /// <summary>
        /// El artículo se presta y debe devolverse (herramienta, arnés). El módulo de
        /// resguardo que lo controla llega después; por ahora sólo se declara.
        /// </summary>
        public bool EsRetornable { get; set; } = false;

        /// <summary>Se entrega por talla (botas, guantes, overol). Un martillo no.</summary>
        public bool RequiereTalla { get; set; } = true;

        /// <summary>
        /// Caduca en almacén con fecha impresa (cascos, arneses, filtros de respirador).
        /// Distinto de VidaUtilDiasDefault, que mide el desgaste en uso.
        /// </summary>
        public bool ControlaCaducidad { get; set; } = false;

        // ===== PROPIEDADES CALCULADAS =====

        /// <summary>Las reglas de consumo y las alertas sólo aplican a equipo de protección.</summary>
        [NotMapped]
        public bool AplicaControlConsumo => TipoMaterial == TipoMaterial.EPP;

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

    /// <summary>
    /// Naturaleza del artículo. Todo lo que existía antes de esta clasificación
    /// es EPP, que es el valor por omisión.
    /// </summary>
    public enum TipoMaterial
    {
        /// <summary>Equipo de protección personal: se entrega a una persona y se controla su consumo.</summary>
        EPP = 1,

        /// <summary>Consumible de obra: se carga a un proyecto o partida, no a una persona.</summary>
        Consumible = 2,

        /// <summary>Herramienta: se resguarda y normalmente se devuelve.</summary>
        Herramienta = 3
    }
}