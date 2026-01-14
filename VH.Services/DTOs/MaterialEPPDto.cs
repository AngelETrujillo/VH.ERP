using System.ComponentModel.DataAnnotations;

namespace VH.Services.DTOs
{
    // DTO para creación/actualización
    public record MaterialEPPRequestDto(
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

        bool Activo
    );

    // DTO para lectura
    public class MaterialEPPResponseDto
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

        // Stock calculado desde Inventarios (se calcula en el servicio)
        public decimal StockGlobal { get; set; }
    }
}