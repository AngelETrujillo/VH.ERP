using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using VH.Services.DTOs.Permiso;

namespace VH.Web.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequierePermisoAttribute : Attribute, IAuthorizationFilter
    {
        public string CodigoModulo { get; }
        public string TipoPermiso { get; }

        public RequierePermisoAttribute(string codigoModulo, string tipoPermiso = "ver")
        {
            CodigoModulo = codigoModulo;
            TipoPermiso = tipoPermiso.ToLower();
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            if (!user.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            // SuperAdmin tiene acceso total
            if (user.IsInRole("SuperAdmin"))
                return;

            // Obtener permisos de la sesión
            var permisosJson = context.HttpContext.Session.GetString("PermisosUsuario");
            if (string.IsNullOrEmpty(permisosJson))
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
                return;
            }

            var permisos = System.Text.Json.JsonSerializer.Deserialize<List<PermisoModuloDto>>(permisosJson,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var permiso = permisos?.FirstOrDefault(p => p.Codigo == CodigoModulo);

            if (permiso == null)
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
                return;
            }

            bool tienePermiso = TipoPermiso switch
            {
                "ver" => permiso.PuedeVer,
                "crear" => permiso.PuedeCrear,
                "editar" => permiso.PuedeEditar,
                "eliminar" => permiso.PuedeEliminar,
                _ => false
            };

            if (!tienePermiso)
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
            }
        }
    }
}