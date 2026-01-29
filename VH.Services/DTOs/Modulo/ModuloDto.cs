namespace VH.Services.DTOs.Modulo
{
    public record ModuloResponseDto(
        int IdModulo,
        string Codigo,
        string Nombre,
        string? Descripcion,
        string? Icono,
        string? ControllerName,
        int Orden,
        bool Activo,
        int? IdModuloPadre,
        string? NombreModuloPadre
    );
}