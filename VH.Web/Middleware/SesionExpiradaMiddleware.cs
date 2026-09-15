using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace VH.Web.Middleware
{
    /// <summary>
    /// Cierra la sesión cuando la cookie sobrevive pero el token no.
    ///
    /// La autenticación de la web vive en una cookie firmada, y el token con el
    /// que se habla con el API vive en la sesión, que es de memoria. Al reiniciar
    /// el sitio la cookie sigue siendo válida y la sesión no: el usuario entraba
    /// como si nada, llegaba al tablero y veía todo vacío, porque cada llamada al
    /// API salía sin credencial y respondía 401 en silencio.
    ///
    /// Ante esa situación lo honesto es pedir que inicie sesión otra vez, no
    /// enseñar pantallas huecas.
    /// </summary>
    public class SesionExpiradaMiddleware
    {
        private readonly RequestDelegate _siguiente;

        public SesionExpiradaMiddleware(RequestDelegate siguiente)
        {
            _siguiente = siguiente;
        }

        public async Task InvokeAsync(HttpContext contexto)
        {
            var ruta = contexto.Request.Path.Value ?? string.Empty;

            // Las pantallas de acceso y los archivos estáticos no se tocan: si no,
            // el propio login quedaría atrapado en el redireccionamiento.
            var esRutaDeAcceso = ruta.StartsWith("/Account", StringComparison.OrdinalIgnoreCase);

            if (!esRutaDeAcceso &&
                contexto.User?.Identity?.IsAuthenticated == true &&
                string.IsNullOrEmpty(contexto.Session.GetString("JwtToken")))
            {
                await contexto.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                contexto.Session.Clear();

                var destino = Uri.EscapeDataString(contexto.Request.Path + contexto.Request.QueryString);
                // "true" y no "1": el enlace de modelo de booleanos no acepta el 1.
                contexto.Response.Redirect($"/Account/Login?ReturnUrl={destino}&sesionExpirada=true");
                return;
            }

            await _siguiente(contexto);
        }
    }
}
