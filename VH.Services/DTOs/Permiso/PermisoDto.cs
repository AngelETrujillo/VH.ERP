namespace VH.Services.DTOs.Permiso
{
    public record RolPermisoResponseDto(
        int IdRolPermiso,
        string IdRol,
        string NombreRol,
        int IdModulo,
        string CodigoModulo,
        string NombreModulo,
        bool PuedeVer,
        bool PuedeCrear,
        bool PuedeEditar,
        bool PuedeEliminar
    );

    public record AsignarPermisoRequestDto(
        string IdRol,
        int IdModulo,
        bool PuedeVer,
        bool PuedeCrear,
        bool PuedeEditar,
        bool PuedeEliminar
    );

    public record PermisoModuloDto(
        int IdModulo,
        string Codigo,
        string Nombre,
        string? Icono,
        int? IdModuloPadre,
        bool PuedeVer,
        bool PuedeCrear,
        bool PuedeEditar,
        bool PuedeEliminar
    );

    public record PermisosRolResponseDto(
        string IdRol,
        string NombreRol,
        List<PermisoModuloDto> Permisos
    );
}