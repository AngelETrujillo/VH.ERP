// VH.Web/Controllers/MaterialesController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VH.Services.DTOs;

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

        // GET: Materiales
        public async Task<IActionResult> Index()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/materiales");
                response.EnsureSuccessStatusCode();
                var materiales = await response.Content.ReadFromJsonAsync<IEnumerable<MaterialEPPResponseDto>>();
                return View(materiales);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar materiales");
                ViewBag.ErrorMessage = "Error al cargar los materiales";
                return View(new List<MaterialEPPResponseDto>());
            }
        }

        // GET: Materiales/Create
        public async Task<IActionResult> Create()
        {
            await CargarUnidadesMedidaEnViewBag();
            return View();
        }

        // POST: Materiales/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MaterialEPPRequestDto materialDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var response = await _httpClient.PostAsJsonAsync("api/materiales", materialDto);
                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction(nameof(Index));
                    }
                    var error = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", $"Error: {error}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al crear material");
                    ModelState.AddModelError("", "Error al crear el material");
                }
            }
            await CargarUnidadesMedidaEnViewBag();
            return View(materialDto);
        }

        // GET: Materiales/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var material = await _httpClient.GetFromJsonAsync<MaterialEPPResponseDto>($"api/materiales/{id}");
                if (material == null) return NotFound();
                return View(material);
            }
            catch
            {
                return NotFound();
            }
        }

        // GET: Materiales/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var material = await _httpClient.GetFromJsonAsync<MaterialEPPResponseDto>($"api/materiales/{id}");
                if (material == null) return NotFound();

                // Convertir a RequestDto
                var dto = new MaterialEPPRequestDto(
                    material.Nombre,
                    material.Descripcion,
                    material.IdUnidadMedida,
                    material.CostoUnitarioEstimado,
                    material.Activo
                );

                await CargarUnidadesMedidaEnViewBag();
                ViewBag.Id = id;
                return View(dto);
            }
            catch
            {
                return NotFound();
            }
        }

        // POST: Materiales/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MaterialEPPRequestDto materialDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var response = await _httpClient.PutAsJsonAsync($"api/materiales/{id}", materialDto);
                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction(nameof(Index));
                    }
                    var error = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", $"Error: {error}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al actualizar material");
                    ModelState.AddModelError("", "Error al actualizar");
                }
            }
            await CargarUnidadesMedidaEnViewBag();
            ViewBag.Id = id;
            return View(materialDto);
        }

        // GET: Materiales/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var material = await _httpClient.GetFromJsonAsync<MaterialEPPResponseDto>($"api/materiales/{id}");
                if (material == null) return NotFound();
                return View(material);
            }
            catch
            {
                return NotFound();
            }
        }

        // POST: Materiales/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/materiales/{id}");
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(Index));
                }
                var error = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"No se pudo eliminar: {error}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar material");
                TempData["ErrorMessage"] = "Error al eliminar";
            }
            return RedirectToAction(nameof(Index));
        }

        // Método auxiliar para cargar unidades de medida
        private async Task CargarUnidadesMedidaEnViewBag()
        {
            try
            {
                var unidades = await _httpClient.GetFromJsonAsync<IEnumerable<UnidadMedidaResponseDto>>("api/unidadesmedida");
                ViewBag.UnidadesMedida = unidades?.Select(u => new SelectListItem
                {
                    Value = u.IdUnidadMedida.ToString(),
                    Text = $"{u.Abreviatura} - {u.Nombre}"
                }).ToList() ?? new List<SelectListItem>();
            }
            catch
            {
                ViewBag.UnidadesMedida = new List<SelectListItem>();
            }
        }
    }
}