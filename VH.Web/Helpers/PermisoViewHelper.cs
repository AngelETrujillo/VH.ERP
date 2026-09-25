using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace VH.Web.Helpers
{
    /// <summary>
    /// Permisos vistos desde una vista de Razor.
    ///
    /// Los filtros de controlador ya impiden entrar a una acción sin permiso, pero
    /// eso sólo se descubre al hacer clic: la pantalla ofrecía botones de crear,
    /// editar y eliminar que terminaban en "acceso denegado". Estas extensiones
    /// permiten no dibujar lo que el usuario no puede hacer, sin tocar el candado
    /// del servidor, que sigue siendo el que manda si alguien escribe la URL.
    /// </summary>
    public static class PermisoViewHelper
    {
        private static bool Tiene(IHtmlHelper html, string modulo, string tipo)
        {
            var contexto = html.ViewContext.HttpContext;
            var esSuperAdmin = contexto.User?.IsInRole("SuperAdmin") ?? false;

            return PermisoHelper.TienePermiso(contexto.Session, modulo, tipo, esSuperAdmin);
        }

        public static bool PuedeVer(this IHtmlHelper html, string modulo) => Tiene(html, modulo, "ver");

        public static bool PuedeCrear(this IHtmlHelper html, string modulo) => Tiene(html, modulo, "crear");

        public static bool PuedeEditar(this IHtmlHelper html, string modulo) => Tiene(html, modulo, "editar");

        public static bool PuedeEliminar(this IHtmlHelper html, string modulo) => Tiene(html, modulo, "eliminar");
    }
}
