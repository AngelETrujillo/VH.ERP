namespace VH.Services.DTOs.LogActividad
{
    public record LogActividadResponseDto(
        int IdLog,
        string IdUsuario,
        string NombreUsuario,
        string Accion,
        string? Entidad,
        int? IdEntidad,
        string? Descripcion,
        string? DireccionIP,
        DateTime Fecha
    );
}