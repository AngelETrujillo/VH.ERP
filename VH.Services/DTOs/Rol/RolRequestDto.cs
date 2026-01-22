using System.ComponentModel.DataAnnotations;

namespace VH.Services.DTOs.Rol
{
    public record RolRequestDto(
        [Required(ErrorMessage = "El nombre del rol es obligatorio")]
        [MaxLength(50)]
        string Name,

        [MaxLength(200)]
        string? Descripcion,

        bool Activo = true
    );
}