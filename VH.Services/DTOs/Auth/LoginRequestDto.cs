using System.ComponentModel.DataAnnotations;

namespace VH.Services.DTOs.Auth
{
    public record LoginRequestDto(
        [Required(ErrorMessage = "El usuario es obligatorio")]
        string Usuario,

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        string Password,

        bool Recordarme = false
    );
}