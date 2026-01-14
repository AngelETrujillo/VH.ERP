using Microsoft.AspNetCore.Mvc;
using VH.Services.DTOs;
using VH.Services.Entities;

namespace VH.Web.Controllers
{
    public class MaterialesController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<MaterialesController> _logger;
        public MaterialesController(IHttpClientFactory httpClientFactory, ILogger<MaterialesController> logger)
        {
            _httpClient = httpClientFactory.CreateClient("ApiERP");
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/materiales");
                response.EnsureSuccessStatusCode();

                var materiales = await response.Content.ReadFromJsonAsync<IEnumerable<MaterialEPPResponseDto>>();
                return View(materiales);
            }
            catch (HttpRequestException e)
            {
                _logger.LogError(e, "Error al conectar con la API");
                ViewBag.ErrorMessage = $"Error  al cargar materiales: {e.Message}";
                return View(new List<MaterialEPPResponseDto>());
            }
        }

        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create([Bind("Nombre,Descripcion,UnidadMedida,RequiereTalla,CostoUnitarioEstimado,Activo")] MaterialEPPResponseDto materialEPPDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var response = await _httpClient.PostAsJsonAsync("api/materiales", materialEPPDto);
                    response.EnsureSuccessStatusCode();

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al crear el material: " + ex.Message);
                }
            }

            return View(materialEPPDto);
        }

        public async Task<IActionResult> Details(int id)
        {
            var material = await _httpClient.GetFromJsonAsync<MaterialEPPResponseDto>($"api/materiales/{id}");
            if (material == null) return NotFound();
            return View(material);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var material = await _httpClient.GetFromJsonAsync<MaterialEPPResponseDto>($"api/materiales/{id}");
            if (material == null) return NotFound();
            return View(material);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MaterialEPPResponseDto materialDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var response = await _httpClient.PutAsJsonAsync($"api/materiales/{id}", materialDto);
                    response.EnsureSuccessStatusCode();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al actualizar material");
                    ModelState.AddModelError("", "Error al actualizar: " + ex.Message);
                }
            }
            return View(materialDto);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var material = await _httpClient.GetFromJsonAsync<MaterialEPPResponseDto>($"api/materiales/{id}");
            if (material == null) return NotFound();
            return View(material);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/materiales/{id}");
                response.EnsureSuccessStatusCode();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar material");
                return View("Error");
            }
        }
    
    }
}
