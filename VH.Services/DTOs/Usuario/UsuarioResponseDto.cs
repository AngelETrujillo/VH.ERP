namespace VH.Services.DTOs.Usuario
{
    public record UsuarioResponseDto(
        string Id,
        string Nombre,
        string ApellidoPaterno,
        string? ApellidoMaterno,
        string NombreCompleto,
        string Email,
        string UserName,
        bool Activo,
        DateTime FechaCreacion,
        DateTime? UltimoAcceso,
        IEnumerable<string> Roles
    );
}