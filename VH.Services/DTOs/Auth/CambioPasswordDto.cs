using System.ComponentModel.DataAnnotations;

namespace VH.Services.DTOs.Auth
{
    public record CambioPasswordDto(
        [Required(ErrorMessage = "La contraseña actual es obligatoria")]
        string PasswordActual,

        [Required(ErrorMessage = "La nueva contraseña es obligatoria")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        string PasswordNuevo,

        [Required(ErrorMessage = "Confirme la nueva contraseña")]
        [Compare("PasswordNuevo", ErrorMessage = "Las contraseñas no coinciden")]
        string ConfirmarPassword
    );
}