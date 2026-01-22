namespace VH.Services.DTOs.Auth
{
    public record LoginResponseDto(
        bool Exitoso,
        string? Token,
        string? RefreshToken,
        DateTime? Expiracion,
        string? Mensaje,
        UsuarioLogueadoDto? Usuario
    );

    public record UsuarioLogueadoDto(
        string Id,
        string NombreCompleto,
        string Email,
        IEnumerable<string> Roles
    );
}