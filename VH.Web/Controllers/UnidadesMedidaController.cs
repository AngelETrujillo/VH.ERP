using Microsoft.AspNetCore.Mvc;
using VH.Services.DTOs;

namespace VH.Web.Controllers
{
    public class UnidadesMedidaController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<UnidadesMedidaController> _logger;

        public UnidadesMedidaController(IHttpClientFactory httpClientFactory, ILogger<UnidadesMedidaController> logger)
        {
            _httpClient = httpClientFactory.CreateClient("ApiERP");
            _logger = logger;
        }

        // GET: UnidadesMedida
        public async Task<IActionResult> Index()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/unidadesmedida");
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
        public IActionResult Create()
        {
            return View();
        }

        // POST: UnidadesMedida/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UnidadMedidaRequestDto dto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var response = await _httpClient.PostAsJsonAsync("api/unidadesmedida", dto);
                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction(nameof(Index));
                    }
                    var error = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", $"Error: {error}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al crear unidad de medida");
                    ModelState.AddModelError("", "Error al crear la unidad de medida");
                }
            }
            return View(dto);
        }

        // GET: UnidadesMedida/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var unidad = await _httpClient.GetFromJsonAsync<UnidadMedidaResponseDto>($"api/unidadesmedida/{id}");
                if (unidad == null) return NotFound();
                return View(unidad);
            }
            catch
            {
                return NotFound();
            }
        }

        // GET: UnidadesMedida/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var unidad = await _httpClient.GetFromJsonAsync<UnidadMedidaResponseDto>($"api/unidadesmedida/{id}");
                if (unidad == null) return NotFound();

                var dto = new UnidadMedidaRequestDto(
                    unidad.Nombre,
                    unidad.Abreviatura,
                    unidad.Descripcion
                );
                ViewBag.Id = id;
                return View(dto);
            }
            catch
            {
                return NotFound();
            }
        }

        // POST: UnidadesMedida/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UnidadMedidaRequestDto dto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var response = await _httpClient.PutAsJsonAsync($"api/unidadesmedida/{id}", dto);
                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction(nameof(Index));
                    }
                    ModelState.AddModelError("", "Error al actualizar");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al actualizar unidad de medida");
                    ModelState.AddModelError("", "Error al actualizar");
                }
            }
            ViewBag.Id = id;
            return View(dto);
        }

        // GET: UnidadesMedida/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var unidad = await _httpClient.GetFromJsonAsync<UnidadMedidaResponseDto>($"api/unidadesmedida/{id}");
                if (unidad == null) return NotFound();
                return View(unidad);
            }
            catch
            {
                return NotFound();
            }
        }

        // POST: UnidadesMedida/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/unidadesmedida/{id}");
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(Index));
                }
                var error = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"No se pudo eliminar: {error}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar unidad de medida");
                TempData["ErrorMessage"] = "Error al eliminar la unidad de medida";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}