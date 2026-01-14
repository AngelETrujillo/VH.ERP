using System.ComponentModel.DataAnnotations;

namespace VH.Services.DTOs
{
    // DTO para creación/actualización
    public record UnidadMedidaRequestDto(
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [MaxLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        string Nombre,

        [Required(ErrorMessage = "La abreviatura es obligatoria")]
        [MaxLength(20, ErrorMessage = "La abreviatura no puede exceder 20 caracteres")]
        string Abreviatura,

        [MaxLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
        string Descripcion
    );

    // DTO para lectura
    public record UnidadMedidaResponseDto(
        int IdUnidadMedida,
        string Nombre,
        string Abreviatura,
        string Descripcion
    );
}