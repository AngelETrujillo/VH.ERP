using Microsoft.AspNetCore.Identity;
using VH.Services.DTOs.Modulo;
using VH.Services.DTOs.Permiso;
using VH.Services.Entities;
using VH.Services.Interfaces;

namespace VH.Services.Services
{
    public class PermisoService : IPermisoService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<Usuario> _userManager;
        private readonly RoleManager<Rol> _roleManager;

        public PermisoService(IUnitOfWork unitOfWork, UserManager<Usuario> userManager, RoleManager<Rol> roleManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IEnumerable<ModuloResponseDto>> GetAllModulosAsync()
        {
            var modulos = await _unitOfWork.Modulos.GetAllAsync(includeProperties: "ModuloPadre");
            return modulos.OrderBy(m => m.Orden).Select(m => new ModuloResponseDto(
                m.IdModulo,
                m.Codigo,
                m.Nombre,
                m.Descripcion,
                m.Icono,
                m.ControllerName,
                m.Orden,
                m.Activo,
                m.IdModuloPadre,
                m.ModuloPadre?.Nombre
            ));
        }

        public async Task<PermisosRolResponseDto?> GetPermisosByRolAsync(string idRol)
        {
            var rol = await _roleManager.FindByIdAsync(idRol);
            if (rol == null) return null;

            var modulos = await _unitOfWork.Modulos.GetAllAsync();
            var permisosExistentes = await _unitOfWork.RolPermisos.FindAsync(rp => rp.IdRol == idRol);

            var permisos = modulos.OrderBy(m => m.Orden).Select(m =>
            {
                var permiso = permisosExistentes.FirstOrDefault(p => p.IdModulo == m.IdModulo);
                return new PermisoModuloDto(
                    m.IdModulo,
                    m.Codigo,
                    m.Nombre,
                    m.Icono,
                    m.IdModuloPadre,
                    permiso?.PuedeVer ?? false,
                    permiso?.PuedeCrear ?? false,
                    permiso?.PuedeEditar ?? false,
                    permiso?.PuedeEliminar ?? false
                );
            }).ToList();

            return new PermisosRolResponseDto(idRol, rol.Name!, permisos);
        }

        public async Task<bool> AsignarPermisosAsync(string idRol, List<AsignarPermisoRequestDto> permisos)
        {
            var rol = await _roleManager.FindByIdAsync(idRol);
            if (rol == null) return false;

            // Eliminar permisos existentes del rol
            var permisosActuales = await _unitOfWork.RolPermisos.FindAsync(rp => rp.IdRol == idRol);
            foreach (var permiso in permisosActuales)
            {
                _unitOfWork.RolPermisos.Remove(permiso);
            }

            // Agregar nuevos permisos
            foreach (var p in permisos)
            {
                if (p.PuedeVer || p.PuedeCrear || p.PuedeEditar || p.PuedeEliminar)
                {
                    var nuevoPermiso = new RolPermiso
                    {
                        IdRol = idRol,
                        IdModulo = p.IdModulo,
                        PuedeVer = p.PuedeVer,
                        PuedeCrear = p.PuedeCrear,
                        PuedeEditar = p.PuedeEditar,
                        PuedeEliminar = p.PuedeEliminar
                    };
                    await _unitOfWork.RolPermisos.AddAsync(nuevoPermiso);
                }
            }

            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<List<PermisoModuloDto>> GetPermisosUsuarioAsync(string userId)
        {
            var usuario = await _userManager.FindByIdAsync(userId);
            if (usuario == null) return new List<PermisoModuloDto>();

            var roles = await _userManager.GetRolesAsync(usuario);
            var rolesIds = new List<string>();

            foreach (var roleName in roles)
            {
                var rol = await _roleManager.FindByNameAsync(roleName);
                if (rol != null) rolesIds.Add(rol.Id);
            }

            var todosPermisos = await _unitOfWork.RolPermisos.FindAsync(
                rp => rolesIds.Contains(rp.IdRol),
                includeProperties: "Modulo"
            );

            // Agrupar permisos por módulo y combinar (OR de todos los roles)
            var permisosAgrupados = todosPermisos
                .GroupBy(p => p.IdModulo)
                .Select(g => new PermisoModuloDto(
                    g.First().Modulo!.IdModulo,
                    g.First().Modulo!.Codigo,
                    g.First().Modulo!.Nombre,
                    g.First().Modulo!.Icono,
                    g.First().Modulo!.IdModuloPadre,
                    g.Any(p => p.PuedeVer),
                    g.Any(p => p.PuedeCrear),
                    g.Any(p => p.PuedeEditar),
                    g.Any(p => p.PuedeEliminar)
                ))
                .ToList();

            return permisosAgrupados;
        }

        public async Task<bool> TienePermisoAsync(string userId, string codigoModulo, string tipoPermiso)
        {
            var permisos = await GetPermisosUsuarioAsync(userId);
            var permiso = permisos.FirstOrDefault(p => p.Codigo == codigoModulo);

            if (permiso == null) return false;

            return tipoPermiso.ToLower() switch
            {
                "ver" => permiso.PuedeVer,
                "crear" => permiso.PuedeCrear,
                "editar" => permiso.PuedeEditar,
                "eliminar" => permiso.PuedeEliminar,
                _ => false
            };
        }
    }
}