using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VH.Services.DTOs;
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

        public async Task<IActionResult> Index(
            int pagina = 1, int tamano = ConsultaPaginada.TamanoPorOmision,
            string? buscar = null)
        {
            var consulta = new ConsultaPaginada { Pagina = pagina, Tamano = tamano, Buscar = buscar };

            SetAuthHeader();
            try
            {
                var url = $"api/Roles/paginado?pagina={consulta.Pagina}&tamano={consulta.Tamano}";
                if (consulta.HayBusqueda) url += $"&buscar={Uri.EscapeDataString(consulta.TextoLimpio!)}";

                var response = await _httpClient.GetAsync(url);
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    return RedirectToAction("Login", "Account");

                var pag = await response.Content.ReadFromJsonAsync<ResultadoPaginado<RolResponseDto>>();
                return View(pag ?? ResultadoPaginado<RolResponseDto>.Ninguno(consulta));
            }
            catch
            {
                ViewBag.Error = "Error al cargar roles";
                return View(ResultadoPaginado<RolResponseDto>.Ninguno(consulta));
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