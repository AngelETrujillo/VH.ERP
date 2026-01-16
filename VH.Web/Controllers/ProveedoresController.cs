// VH.Web/Controllers/ProveedoresController.cs
using Microsoft.AspNetCore.Mvc;
using VH.Services.DTOs;

namespace VH.Web.Controllers
{
    public class ProveedoresController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ProveedoresController> _logger;

        public ProveedoresController(IHttpClientFactory httpClientFactory, ILogger<ProveedoresController> logger)
        {
            _httpClient = httpClientFactory.CreateClient("ApiERP");
            _logger = logger;
        }

        // GET: Proveedores
        public async Task<IActionResult> Index()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/proveedores");
                response.EnsureSuccessStatusCode();
                var proveedores = await response.Content.ReadFromJsonAsync<IEnumerable<ProveedorResponseDto>>();
                return View(proveedores);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar proveedores");
                ViewBag.ErrorMessage = "Error al cargar los proveedores";
                return View(new List<ProveedorResponseDto>());
            }
        }

        // GET: Proveedores/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Proveedores/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProveedorRequestDto proveedorDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var response = await _httpClient.PostAsJsonAsync("api/proveedores", proveedorDto);
                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction(nameof(Index));
                    }
                    var error = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", $"Error: {error}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al crear proveedor");
                    ModelState.AddModelError("", "Error al crear el proveedor");
                }
            }
            return View(proveedorDto);
        }

        // GET: Proveedores/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var proveedor = await _httpClient.GetFromJsonAsync<ProveedorResponseDto>($"api/proveedores/{id}");
                if (proveedor == null) return NotFound();
                return View(proveedor);
            }
            catch
            {
                return NotFound();
            }
        }

        // GET: Proveedores/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var proveedor = await _httpClient.GetFromJsonAsync<ProveedorResponseDto>($"api/proveedores/{id}");
                if (proveedor == null) return NotFound();

                // Convertir a RequestDto
                var dto = new ProveedorRequestDto(
                    proveedor.Nombre,
                    proveedor.RFC,
                    proveedor.Contacto,
                    proveedor.Telefono,
                    proveedor.Activo
                );
                ViewBag.Id = id;
                return View(dto);
            }
            catch
            {
                return NotFound();
            }
        }

        // POST: Proveedores/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProveedorRequestDto proveedorDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var response = await _httpClient.PutAsJsonAsync($"api/proveedores/{id}", proveedorDto);
                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction(nameof(Index));
                    }
                    ModelState.AddModelError("", "Error al actualizar");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al actualizar el proveedor");
                    ModelState.AddModelError("", "Error al actualizar: " + ex.Message);
                }
            }
            ViewBag.Id = id;
            return View(proveedorDto);
        }

        // GET: Proveedores/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var proveedor = await _httpClient.GetFromJsonAsync<ProveedorResponseDto>($"api/proveedores/{id}");
                if (proveedor == null) return NotFound();
                return View(proveedor);
            }
            catch
            {
                return NotFound();
            }
        }

        // POST: Proveedores/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/proveedores/{id}");
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(Index));
                }
                TempData["ErrorMessage"] = "No se pudo eliminar el proveedor";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar proveedor");
                TempData["ErrorMessage"] = "Error al eliminar";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}