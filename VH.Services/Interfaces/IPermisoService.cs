using VH.Services.DTOs.Modulo;
using VH.Services.DTOs.Permiso;

namespace VH.Services.Interfaces
{
    public interface IPermisoService
    {
        Task<IEnumerable<ModuloResponseDto>> GetAllModulosAsync();
        Task<PermisosRolResponseDto?> GetPermisosByRolAsync(string idRol);
        Task<bool> AsignarPermisosAsync(string idRol, List<AsignarPermisoRequestDto> permisos);
        Task<List<PermisoModuloDto>> GetPermisosUsuarioAsync(string userId);
        Task<bool> TienePermisoAsync(string userId, string codigoModulo, string tipoPermiso);
    }
}