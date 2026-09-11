using System.Net.Http.Headers;

namespace VH.Web.Handlers
{
    /// <summary>
    /// Adjunta el JWT guardado en sesión a cada llamada al cliente "ApiERP".
    ///
    /// Hasta ahora cada controlador lo hacía a mano con su propio SetAuthHeader(),
    /// y ProyectosController se quedó sin hacerlo. Mientras la API estuvo abierta
    /// eso no se notaba; ahora que exige token, un olvido así deja una pantalla
    /// entera fuera de servicio.
    ///
    /// Si el controlador ya puso el encabezado, se respeta el suyo.
    /// </summary>
    public class TokenSesionHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TokenSesionHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Headers.Authorization == null)
            {
                var token = _httpContextAccessor.HttpContext?.Session?.GetString("JwtToken");

                if (!string.IsNullOrEmpty(token))
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
