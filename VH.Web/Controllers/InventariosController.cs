using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VH.Services.DTOs;

namespace VH.Web.Controllers
{
    public class InventariosController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<InventariosController> _logger;

        public InventariosController(IHttpClientFactory httpClientFactory, ILogger<InventariosController> logger)
        {
            _httpClient = httpClientFactory.CreateClient("ApiERP");
            _logger = logger;
        }

        // GET: Inventarios
        public async Task<IActionResult> Index()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/inventarios");
                response.EnsureSuccessStatusCode();
                var inventarios = await response.Content.ReadFromJsonAsync<IEnumerable<InventarioResponseDto>>();
                return View(inventarios);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar inventarios");
                ViewBag.ErrorMessage = "Error al cargar los inventarios";
                return View(new List<InventarioResponseDto>());
            }
        }

        // GET: Inventarios/Create
        public async Task<IActionResult> Create()
        {
            await CargarListasEnViewBag();
            return View();
        }

        // POST: Inventarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InventarioRequestDto dto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var response = await _httpClient.PostAsJsonAsync("api/inventarios", dto);
                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction(nameof(Index));
                    }
                    var error = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", $"Error: {error}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al crear inventario");
                    ModelState.AddModelError("", "Error al crear el registro de inventario");
                }
            }
            await CargarListasEnViewBag();
            return View(dto);
        }

        // GET: Inventarios/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var inventario = await _httpClient.GetFromJsonAsync<InventarioResponseDto>($"api/inventarios/{id}");
                if (inventario == null) return NotFound();

                var dto = new InventarioRequestDto(
                    inventario.IdAlmacen,
                    inventario.IdMaterial,
                    inventario.Existencia,
                    inventario.StockMinimo,
                    inventario.StockMaximo,
                    inventario.UbicacionPasillo
                );

                await CargarListasEnViewBag();
                ViewBag.Id = id;
                ViewBag.NombreAlmacen = inventario.NombreAlmacen;
                ViewBag.NombreMaterial = inventario.NombreMaterial;
                return View(dto);
            }
            catch
            {
                return NotFound();
            }
        }

        // POST: Inventarios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, InventarioRequestDto dto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var response = await _httpClient.PutAsJsonAsync($"api/inventarios/{id}", dto);
                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction(nameof(Index));
                    }
                    var error = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", $"Error: {error}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al actualizar inventario");
                    ModelState.AddModelError("", "Error al actualizar");
                }
            }
            await CargarListasEnViewBag();
            ViewBag.Id = id;
            return View(dto);
        }

        // GET: Inventarios/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var inventario = await _httpClient.GetFromJsonAsync<InventarioResponseDto>($"api/inventarios/{id}");
                if (inventario == null) return NotFound();
                return View(inventario);
            }
            catch
            {
                return NotFound();
            }
        }

        // POST: Inventarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/inventarios/{id}");
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(Index));
                }
                TempData["ErrorMessage"] = "No se pudo eliminar el registro";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar inventario");
                TempData["ErrorMessage"] = "Error al eliminar";
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task CargarListasEnViewBag()
        {
            try
            {
                var almacenes = await _httpClient.GetFromJsonAsync<IEnumerable<AlmacenResponseDto>>("api/almacenes");
                ViewBag.Almacenes = almacenes?.Where(a => a.Activo).Select(a => new SelectListItem
                {
                    Value = a.IdAlmacen.ToString(),
                    Text = $"{a.Nombre} ({a.NombreProyecto})"
                }).ToList() ?? new List<SelectListItem>();

                var materiales = await _httpClient.GetFromJsonAsync<IEnumerable<MaterialEPPResponseDto>>("api/materiales");
                ViewBag.Materiales = materiales?.Where(m => m.Activo).Select(m => new SelectListItem
                {
                    Value = m.IdMaterial.ToString(),
                    Text = $"{m.Nombre} ({m.AbreviaturaUnidadMedida})"
                }).ToList() ?? new List<SelectListItem>();
            }
            catch
            {
                ViewBag.Almacenes = new List<SelectListItem>();
                ViewBag.Materiales = new List<SelectListItem>();
            }
        }
    }
}