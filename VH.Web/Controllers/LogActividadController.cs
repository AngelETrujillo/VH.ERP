using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public async Task<IActionResult> Index(DateTime? desde, DateTime? hasta, string? userId)
        {
            SetAuthHeader();
            try
            {
                var url = "api/LogActividad";
                var queryParams = new List<string>();

                if (desde.HasValue) queryParams.Add($"desde={desde:yyyy-MM-dd}");
                if (hasta.HasValue) queryParams.Add($"hasta={hasta:yyyy-MM-dd}");
                if (!string.IsNullOrEmpty(userId)) queryParams.Add($"userId={userId}");

                if (queryParams.Any()) url += "?" + string.Join("&", queryParams);

                var response = await _httpClient.GetAsync(url);
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    return RedirectToAction("Login", "Account");

                var logs = await response.Content.ReadFromJsonAsync<IEnumerable<LogActividadResponseDto>>();

                ViewBag.Desde = desde;
                ViewBag.Hasta = hasta;
                ViewBag.UserId = userId;

                return View(logs);
            }
            catch
            {
                ViewBag.Error = "Error al cargar logs";
                return View(new List<LogActividadResponseDto>());
            }
        }
    }
}