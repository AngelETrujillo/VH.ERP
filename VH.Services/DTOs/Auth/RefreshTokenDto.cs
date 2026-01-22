using System.ComponentModel.DataAnnotations;

namespace VH.Services.DTOs.Auth
{
    public record RefreshTokenDto(
        [Required] string Token,
        [Required] string RefreshToken
    );
}