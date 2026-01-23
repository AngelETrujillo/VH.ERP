using System.ComponentModel.DataAnnotations;

namespace VH.Services.DTOs.Auth
{
    public class CambioPasswordDto
    {
        [Required(ErrorMessage = "La contraseña actual es obligatoria")]
        public string PasswordActual { get; set; } = string.Empty;

        [Required(ErrorMessage = "La nueva contraseña es obligatoria")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string PasswordNuevo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirme la nueva contraseña")]
        [Compare("PasswordNuevo", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmarPassword { get; set; } = string.Empty;
    }
}