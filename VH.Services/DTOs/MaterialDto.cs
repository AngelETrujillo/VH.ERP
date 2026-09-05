using System.ComponentModel.DataAnnotations;
using VH.Services.Entities;

namespace VH.Services.DTOs
{
    // DTO para creación/actualización
    public record MaterialRequestDto(
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [MaxLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres")]
        string Nombre,

        [MaxLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
        string Descripcion,

        [Required(ErrorMessage = "La unidad de medida es obligatoria")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una unidad de medida válida")]
        int IdUnidadMedida,

        [Required(ErrorMessage = "El costo unitario es obligatorio")]
        [Range(0, double.MaxValue, ErrorMessage = "El costo debe ser mayor o igual a 0")]
        decimal CostoUnitarioEstimado,

        bool Activo,

        [Required(ErrorMessage = "El tipo de material es obligatorio")]
        TipoMaterial TipoMaterial = TipoMaterial.EPP,

        bool RequiereTalla = true,

        bool EsRetornable = false,

        bool ControlaCaducidad = false
    );

    // DTO para lectura
    public class MaterialResponseDto
    {
        public int IdMaterial { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;

        // Datos de UnidadMedida
        public int IdUnidadMedida { get; set; }
        public string NombreUnidadMedida { get; set; } = string.Empty;
        public string AbreviaturaUnidadMedida { get; set; } = string.Empty;

        public decimal CostoUnitarioEstimado { get; set; }
        public bool Activo { get; set; }

        // Clasificación
        public TipoMaterial TipoMaterial { get; set; }
        public bool RequiereTalla { get; set; }
        public bool EsRetornable { get; set; }
        public bool ControlaCaducidad { get; set; }

        /// <summary>Etiqueta legible del tipo, para listados y detalles.</summary>
        public string TipoMaterialTexto => TipoMaterial switch
        {
            TipoMaterial.EPP => "EPP",
            TipoMaterial.Consumible => "Consumible",
            TipoMaterial.Herramienta => "Herramienta",
            _ => TipoMaterial.ToString()
        };

        /// <summary>Color Bootstrap del distintivo de tipo.</summary>
        public string TipoMaterialClase => TipoMaterial switch
        {
            TipoMaterial.EPP => "primary",
            TipoMaterial.Consumible => "secondary",
            TipoMaterial.Herramienta => "dark",
            _ => "secondary"
        };

        /// <summary>Sólo el EPP entra al motor de alertas de consumo.</summary>
        public bool AplicaControlConsumo => TipoMaterial == TipoMaterial.EPP;

        // Stock calculado desde Inventarios (se calcula en el servicio)
        public decimal StockGlobal { get; set; }
    }
}