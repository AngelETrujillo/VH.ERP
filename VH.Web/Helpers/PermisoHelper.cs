using VH.Services.DTOs.Permiso;
using System.Text.Json;

namespace VH.Web.Helpers
{
    public static class PermisoHelper
    {
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public static List<PermisoModuloDto> ObtenerPermisos(ISession session)
        {
            var permisosJson = session.GetString("PermisosUsuario");
            if (string.IsNullOrEmpty(permisosJson))
                return new List<PermisoModuloDto>();

            return JsonSerializer.Deserialize<List<PermisoModuloDto>>(permisosJson, _jsonOptions)
                   ?? new List<PermisoModuloDto>();
        }

        public static bool TienePermiso(ISession session, string codigoModulo, string tipoPermiso, bool esSuperAdmin)
        {
            if (esSuperAdmin) return true;

            var permisos = ObtenerPermisos(session);
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

        public static bool PuedeVer(ISession session, string codigoModulo, bool esSuperAdmin)
            => TienePermiso(session, codigoModulo, "ver", esSuperAdmin);

        public static bool PuedeCrear(ISession session, string codigoModulo, bool esSuperAdmin)
            => TienePermiso(session, codigoModulo, "crear", esSuperAdmin);

        public static bool PuedeEditar(ISession session, string codigoModulo, bool esSuperAdmin)
            => TienePermiso(session, codigoModulo, "editar", esSuperAdmin);

        public static bool PuedeEliminar(ISession session, string codigoModulo, bool esSuperAdmin)
            => TienePermiso(session, codigoModulo, "eliminar", esSuperAdmin);
    }
}