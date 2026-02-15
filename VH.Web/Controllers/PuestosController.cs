using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VH.Services.DTOs.Analytics;
using VH.Services.Entities;
using VH.Web.Filters;

namespace VH.Web.Controllers
{
    [Authorize]
    [RequierePermiso("PUESTOS", "ver")]
    public class PuestosController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PuestosController> _logger;

        public PuestosController(IHttpClientFactory httpClientFactory, ILogger<PuestosController> logger)
        {
            _httpClient = httpClientFactory.CreateClient("ApiERP");
            _logger = logger;
        }

        private void SetAuthHeader()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            _logger.LogWarning("=== DEBUG: Token en sesión = {Token}",
                string.IsNullOrEmpty(token) ? "NULL/VACÍO" : token.Substring(0, Math.Min(50, token.Length)) + "...");

            if (!string.IsNullOrEmpty(token))
            {
                // Limpiar headers anteriores antes de agregar
                _httpClient.DefaultRequestHeaders.Authorization = null;
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
        }

        //public async Task<IActionResult> Index()
        //{
        //    SetAuthHeader();
        //    try
        //    {
        //        var puestos = await _httpClient.GetFromJsonAsync<IEnumerable<PuestoResponseDto>>("api/puestos");
        //        return View(puestos);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error al cargar puestos");
        //        TempData["Error"] = "Error al cargar los puestos";
        //        return View(new List<PuestoResponseDto>());
        //    }
        //}

        //public async Task<IActionResult> Index()
        //{
        //    SetAuthHeader();
        //    try
        //    {
        //        var token = HttpContext.Session.GetString("JwtToken");
        //        _logger.LogInformation("Token presente: {HasToken}", !string.IsNullOrEmpty(token));

        //        var response = await _httpClient.GetAsync("api/puestos");
        //        var content = await response.Content.ReadAsStringAsync();

        //        _logger.LogInformation("Status: {Status}, Content: {Content}", response.StatusCode, content);

        //        if (!response.IsSuccessStatusCode)
        //        {
        //            TempData["Error"] = $"Error API: {response.StatusCode} - {content}";
        //            return View(new List<PuestoResponseDto>());
        //        }

        //        var puestos = System.Text.Json.JsonSerializer.Deserialize<IEnumerable<PuestoResponseDto>>(
        //            content,
        //            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        //        );

        //        _logger.LogInformation("Puestos encontrados: {Count}", puestos?.Count() ?? 0);
        //        return View(puestos ?? new List<PuestoResponseDto>());
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error al cargar puestos");
        //        TempData["Error"] = $"Excepción: {ex.Message}";
        //        return View(new List<PuestoResponseDto>());
        //    }
        //}

        public async Task<IActionResult> Index()
        {
            SetAuthHeader();
            try
            {
                // LOG TEMPORAL - Ver URL base
                _logger.LogWarning("=== DEBUG: BaseAddress = {Base}", _httpClient.BaseAddress);

                var response = await _httpClient.GetAsync("api/puestos");
                var content = await response.Content.ReadAsStringAsync();

                _logger.LogWarning("=== DEBUG: Status = {Status}", response.StatusCode);
                _logger.LogWarning("=== DEBUG: Content (primeros 500 chars) = {Content}",
                    content.Length > 500 ? content.Substring(0, 500) : content);

                if (!response.IsSuccessStatusCode)
                {
                    TempData["Error"] = $"API Error: {response.StatusCode}";
                    return View(new List<PuestoResponseDto>());
                }

                var puestos = await response.Content.ReadFromJsonAsync<IEnumerable<PuestoResponseDto>>();
                return View(puestos ?? new List<PuestoResponseDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar puestos");
                TempData["Error"] = ex.Message;
                return View(new List<PuestoResponseDto>());
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            SetAuthHeader();
            try
            {
                var puesto = await _httpClient.GetFromJsonAsync<PuestoResponseDto>($"api/puestos/{id}");
                if (puesto == null) return NotFound();
                return View(puesto);
            }
            catch
            {
                return NotFound();
            }
        }

        [RequierePermiso("PUESTOS", "crear")]
        public IActionResult Create()
        {
            ViewBag.NivelesRiesgo = GetNivelesRiesgoSelectList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("PUESTOS", "crear")]
        public async Task<IActionResult> Create(PuestoRequestDto dto)
        {
            // DEBUG: Ver qué valores llegan
            _logger.LogWarning("=== DEBUG CREATE: Nombre={Nombre}, Activo={Activo}, NivelRiesgo={Nivel}",
                dto.Nombre, dto.Activo, dto.NivelRiesgoEPP);

            SetAuthHeader();
            if (!ModelState.IsValid)
            {
                ViewBag.NivelesRiesgo = GetNivelesRiesgoSelectList();
                return View(dto);
            }

            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/puestos", dto);
                if (response.IsSuccessStatusCode)
                {
                    TempData["Mensaje"] = "Puesto creado correctamente";
                    return RedirectToAction(nameof(Index));
                }

                var error = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError("", error);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear puesto");
                ModelState.AddModelError("", "Error al crear el puesto");
            }

            ViewBag.NivelesRiesgo = GetNivelesRiesgoSelectList();
            return View(dto);
        }

        [RequierePermiso("PUESTOS", "editar")]
        public async Task<IActionResult> Edit(int id)
        {
            SetAuthHeader();
            try
            {
                var puesto = await _httpClient.GetFromJsonAsync<PuestoResponseDto>($"api/puestos/{id}");
                if (puesto == null) return NotFound();

                var dto = new PuestoRequestDto(
                    puesto.Nombre,
                    puesto.Descripcion,
                    puesto.NivelRiesgoEPP,
                    puesto.Activo
                );

                ViewBag.Id = id;
                ViewBag.NivelesRiesgo = GetNivelesRiesgoSelectList();
                return View(dto);
            }
            catch
            {
                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("PUESTOS", "editar")]
        public async Task<IActionResult> Edit(int id, PuestoRequestDto dto)
        {
            SetAuthHeader();
            if (!ModelState.IsValid)
            {
                ViewBag.Id = id;
                ViewBag.NivelesRiesgo = GetNivelesRiesgoSelectList();
                return View(dto);
            }

            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/puestos/{id}", dto);
                if (response.IsSuccessStatusCode)
                {
                    TempData["Mensaje"] = "Puesto actualizado correctamente";
                    return RedirectToAction(nameof(Index));
                }

                var error = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError("", error);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar puesto {Id}", id);
                ModelState.AddModelError("", "Error al actualizar el puesto");
            }

            ViewBag.Id = id;
            ViewBag.NivelesRiesgo = GetNivelesRiesgoSelectList();
            return View(dto);
        }

        [RequierePermiso("PUESTOS", "eliminar")]
        public async Task<IActionResult> Delete(int id)
        {
            SetAuthHeader();
            try
            {
                var puesto = await _httpClient.GetFromJsonAsync<PuestoResponseDto>($"api/puestos/{id}");
                if (puesto == null) return NotFound();
                return View(puesto);
            }
            catch
            {
                return NotFound();
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [RequierePermiso("PUESTOS", "eliminar")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            SetAuthHeader();
            try
            {
                var response = await _httpClient.DeleteAsync($"api/puestos/{id}");
                if (response.IsSuccessStatusCode)
                {
                    TempData["Mensaje"] = "Puesto eliminado correctamente";
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    TempData["Error"] = error;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar puesto {Id}", id);
                TempData["Error"] = "Error al eliminar el puesto";
            }

            return RedirectToAction(nameof(Index));
        }

        private static List<SelectListItem> GetNivelesRiesgoSelectList()
        {
            return new List<SelectListItem>
            {
                new() { Value = "1", Text = "Bajo" },
                new() { Value = "2", Text = "Medio" },
                new() { Value = "3", Text = "Alto" }
            };
        }
    }
}
