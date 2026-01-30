using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VH.Services.DTOs;
using VH.Web.Filters;

namespace VH.Web.Controllers
{
    [Authorize]
    [RequierePermiso("UNIDADES_MEDIDA", "ver")]
    public class UnidadesMedidaController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<UnidadesMedidaController> _logger;

        public UnidadesMedidaController(IHttpClientFactory httpClientFactory, ILogger<UnidadesMedidaController> logger)
        {
            _httpClient = httpClientFactory.CreateClient("ApiERP");
            _logger = logger;
        }

        private void SetAuthHeader()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        // GET: UnidadesMedida
        public async Task<IActionResult> Index()
        {
            SetAuthHeader();
            try
            {
                var response = await _httpClient.GetAsync("api/unidadesmedida");
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    return RedirectToAction("Login", "Account");

                response.EnsureSuccessStatusCode();
                var unidades = await response.Content.ReadFromJsonAsync<IEnumerable<UnidadMedidaResponseDto>>();
                return View(unidades);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar unidades de medida");
                ViewBag.ErrorMessage = "Error al cargar las unidades de medida";
                return View(new List<UnidadMedidaResponseDto>());
            }
        }

        // GET: UnidadesMedida/Create
        [RequierePermiso("UNIDADES_MEDIDA", "crear")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: UnidadesMedida/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("UNIDADES_MEDIDA", "crear")]
        public async Task<IActionResult> Create(UnidadMedidaRequestDto dto)
        {
            if (ModelState.IsValid)
            {
                SetAuthHeader();
                try
                {
                    var response = await _httpClient.PostAsJsonAsync("api/unidadesmedida", dto);
                    if (response.IsSuccessStatusCode)
                    {
                        TempData["Mensaje"] = "Unidad de medida creada exitosamente";
                        return RedirectToAction(nameof(Index));
                    }
                    var error = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", error);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al crear unidad de medida");
                    ModelState.AddModelError("", "Error al crear la unidad de medida");
                }
            }
            return View(dto);
        }

        // GET: UnidadesMedida/Edit/5
        [RequierePermiso("UNIDADES_MEDIDA", "editar")]
        public async Task<IActionResult> Edit(int id)
        {
            SetAuthHeader();
            var response = await _httpClient.GetAsync($"api/unidadesmedida/{id}");
            if (!response.IsSuccessStatusCode) return NotFound();

            var unidad = await response.Content.ReadFromJsonAsync<UnidadMedidaResponseDto>();
            ViewBag.IdUnidadMedida = id;
            return View(unidad);
        }

        // POST: UnidadesMedida/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("UNIDADES_MEDIDA", "editar")]
        public async Task<IActionResult> Edit(int id, UnidadMedidaRequestDto dto)
        {
            if (ModelState.IsValid)
            {
                SetAuthHeader();
                var response = await _httpClient.PutAsJsonAsync($"api/unidadesmedida/{id}", dto);
                if (response.IsSuccessStatusCode)
                {
                    TempData["Mensaje"] = "Unidad de medida actualizada exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", "Error al actualizar");
            }
            ViewBag.IdUnidadMedida = id;
            return View(dto);
        }

        // POST: UnidadesMedida/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("UNIDADES_MEDIDA", "eliminar")]
        public async Task<IActionResult> Delete(int id)
        {
            SetAuthHeader();
            var response = await _httpClient.DeleteAsync($"api/unidadesmedida/{id}");
            if (response.IsSuccessStatusCode)
                TempData["Mensaje"] = "Unidad de medida eliminada";
            else
                TempData["Error"] = "No se pudo eliminar la unidad de medida";

            return RedirectToAction(nameof(Index));
        }
    }
}