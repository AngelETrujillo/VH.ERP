namespace VH.Services.DTOs.Rol
{
    public record RolResponseDto(
        string Id,
        string Name,
        string? Descripcion,
        bool Activo,
        DateTime FechaCreacion
    );
}