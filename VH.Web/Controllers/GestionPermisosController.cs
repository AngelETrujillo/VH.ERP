using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VH.Services.DTOs.Permiso;
using VH.Services.DTOs.Rol;

namespace VH.Web.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class GestionPermisosController : Controller
    {
        private readonly HttpClient _httpClient;

        public GestionPermisosController(IHttpClientFactory httpClientFactory)
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

        public async Task<IActionResult> Index()
        {
            SetAuthHeader();
            try
            {
                var response = await _httpClient.GetAsync("api/Roles");
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    return RedirectToAction("Login", "Account");

                var roles = await response.Content.ReadFromJsonAsync<IEnumerable<RolResponseDto>>();
                return View(roles);
            }
            catch
            {
                ViewBag.Error = "Error al cargar roles";
                return View(new List<RolResponseDto>());
            }
        }

        public async Task<IActionResult> Asignar(string id)
        {
            SetAuthHeader();
            try
            {
                var response = await _httpClient.GetAsync($"api/Permisos/rol/{id}");
                if (!response.IsSuccessStatusCode) return NotFound();

                var permisos = await response.Content.ReadFromJsonAsync<PermisosRolResponseDto>();
                return View(permisos);
            }
            catch
            {
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Asignar(string id, List<AsignarPermisoRequestDto> permisos)
        {
            SetAuthHeader();
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"api/Permisos/rol/{id}", permisos);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Mensaje"] = "Permisos asignados exitosamente";
                    return RedirectToAction(nameof(Index));
                }

                TempData["Error"] = "Error al asignar permisos";
                return RedirectToAction(nameof(Asignar), new { id });
            }
            catch
            {
                TempData["Error"] = "Error de conexión";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}