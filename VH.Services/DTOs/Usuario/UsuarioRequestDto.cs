using System.ComponentModel.DataAnnotations;

namespace VH.Services.DTOs.Usuario
{
    public record UsuarioRequestDto(
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [MaxLength(100)]
        string Nombre,

        [Required(ErrorMessage = "El apellido paterno es obligatorio")]
        [MaxLength(100)]
        string ApellidoPaterno,

        [MaxLength(100)]
        string? ApellidoMaterno,

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Email no válido")]
        string Email,

        [Required(ErrorMessage = "El nombre de usuario es obligatorio")]
        [MaxLength(50)]
        string UserName,

        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        string? Password,

        List<string>? Roles,

        bool Activo = true
    );
}