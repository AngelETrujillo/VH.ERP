using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VH.Services.DTOs;
using VH.Services.DTOs.LogActividad;

namespace VH.Web.Controllers
{
    [Authorize(Roles = "SuperAdmin,Administrador")]
    public class LogActividadController : Controller
    {
        private readonly HttpClient _httpClient;

        public LogActividadController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("ApiERP");
        }

        private void SetAuthHeader()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<IActionResult> Index(
            DateTime? desde, DateTime? hasta, string? userId,
            int pagina = 1, int tamano = ConsultaPaginada.TamanoPorOmision, string? buscar = null)
        {
            SetAuthHeader();

            var consulta = new ConsultaPaginada { Pagina = pagina, Tamano = tamano, Buscar = buscar };

            ViewBag.Desde = desde;
            ViewBag.Hasta = hasta;
            ViewBag.UserId = userId;

            try
            {
                var partes = new List<string>
                {
                    $"pagina={consulta.Pagina}",
                    $"tamano={consulta.Tamano}"
                };

                if (consulta.HayBusqueda) partes.Add($"buscar={Uri.EscapeDataString(consulta.TextoLimpio!)}");
                if (desde.HasValue) partes.Add($"desde={desde:yyyy-MM-dd}");
                if (hasta.HasValue) partes.Add($"hasta={hasta:yyyy-MM-dd}");
                if (!string.IsNullOrEmpty(userId)) partes.Add($"userId={Uri.EscapeDataString(userId)}");

                var url = "api/LogActividad/paginado?" + string.Join("&", partes);

                var response = await _httpClient.GetAsync(url);
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    return RedirectToAction("Login", "Account");

                response.EnsureSuccessStatusCode();
                var pag = await response.Content.ReadFromJsonAsync<ResultadoPaginado<LogActividadResponseDto>>();

                return View(pag ?? ResultadoPaginado<LogActividadResponseDto>.Ninguno(consulta));
            }
            catch
            {
                ViewBag.Error = "Error al cargar logs";
                return View(ResultadoPaginado<LogActividadResponseDto>.Ninguno(consulta));
            }
        }
    }
}