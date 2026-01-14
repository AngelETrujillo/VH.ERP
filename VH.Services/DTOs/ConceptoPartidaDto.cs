using System.ComponentModel.DataAnnotations;

namespace VH.Services.DTOs
{
    // DTO para respuestas (lectura)
    public class ConceptoPartidaResponseDto
    {
        public int IdPartida { get; set; }
        public int IdProyecto { get; set; }
        public string Descripcion { get; set; } = string.Empty;

        // Datos de UnidadMedida
        public int IdUnidadMedida { get; set; }
        public string NombreUnidadMedida { get; set; } = string.Empty;
        public string AbreviaturaUnidadMedida { get; set; } = string.Empty;

        public decimal CantidadEstimada { get; set; }
        public decimal? CostoTotalEstimado { get; set; }
    }

    // DTO para creación (escritura)
    public record ConceptoPartidaRequestDto(
        [Required(ErrorMessage = "La descripción es obligatoria")]
        [MaxLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
        string Descripcion,

        [Required(ErrorMessage = "La unidad de medida es obligatoria")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una unidad de medida válida")]
        int IdUnidadMedida,

        [Required(ErrorMessage = "La cantidad estimada es obligatoria")]
        [Range(0.01, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        decimal CantidadEstimada
    );
}