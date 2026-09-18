using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using VH.Services.Interfaces;

namespace VH.API.Filters
{
    /// <summary>
    /// Valida la matriz de permisos por módulo del lado del servidor.
    ///
    /// Hasta ahora los permisos granulares (ver/crear/editar/eliminar por módulo)
    /// sólo se evaluaban en VH.Web, de modo que cualquier cliente que llamara
    /// directo a la API se los saltaba. Este filtro los aplica en la API, que es
    /// donde no se pueden esquivar.
    ///
    /// Se combina con [Authorize]: ese atributo verifica que haya un token válido,
    /// éste verifica que el usuario del token pueda hacer la operación.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequierePermisoApiAttribute : Attribute, IAsyncAuthorizationFilter
    {
        public string CodigoModulo { get; }
        public string TipoPermiso { get; }

        public RequierePermisoApiAttribute(string codigoModulo, string tipoPermiso = "ver")
        {
            CodigoModulo = codigoModulo;
            TipoPermiso = tipoPermiso.ToLowerInvariant();
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            if (user.Identity?.IsAuthenticated != true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // SuperAdmin tiene acceso total, igual que en VH.Web.
            if (user.IsInRole("SuperAdmin"))
                return;

            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Un filtro no puede recibir servicios por constructor cuando se usa
            // como atributo, así que se resuelven del contenedor de la petición.
            var permisoService = context.HttpContext.RequestServices.GetService<IPermisoService>();
            if (permisoService == null)
            {
                context.Result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
                return;
            }

            var tienePermiso = await permisoService.TienePermisoAsync(userId, CodigoModulo, TipoPermiso);

            if (!tienePermiso)
            {
                context.Result = new ObjectResult(new
                {
                    mensaje = $"No tiene permiso para {TipoPermiso} en el módulo {CodigoModulo}."
                })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
            }
        }
    }
}
