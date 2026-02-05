using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VH.Services.DTOs;
using VH.Web.Filters;

namespace VH.Web.Controllers
{
    [Authorize]
    [RequierePermiso("PROVEEDORES", "ver")]
    public class ProveedoresController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ProveedoresController> _logger;

        public ProveedoresController(IHttpClientFactory httpClientFactory, ILogger<ProveedoresController> logger)
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

        // GET: Proveedores
        public async Task<IActionResult> Index()
        {
            SetAuthHeader();
            try
            {
                var response = await _httpClient.GetAsync("api/proveedores");
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    return RedirectToAction("Login", "Account");

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

        // GET: Proveedores/Details/5
        public async Task<IActionResult> Details(int id)
        {
            SetAuthHeader();
            var response = await _httpClient.GetAsync($"api/proveedores/{id}");
            if (!response.IsSuccessStatusCode) return NotFound();

            var proveedor = await response.Content.ReadFromJsonAsync<ProveedorResponseDto>();
            return View(proveedor);
        }

        // GET: Proveedores/Create
        [RequierePermiso("PROVEEDORES", "crear")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Proveedores/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("PROVEEDORES", "crear")]
        public async Task<IActionResult> Create(ProveedorRequestDto proveedorDto)
        {
            if (ModelState.IsValid)
            {
                SetAuthHeader();
                try
                {
                    var response = await _httpClient.PostAsJsonAsync("api/proveedores", proveedorDto);
                    if (response.IsSuccessStatusCode)
                    {
                        TempData["Mensaje"] = "Proveedor creado exitosamente";
                        return RedirectToAction(nameof(Index));
                    }
                    var error = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", error);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al crear proveedor");
                    ModelState.AddModelError("", "Error al crear el proveedor");
                }
            }
            return View(proveedorDto);
        }

        // GET: Proveedores/Edit/5
        [RequierePermiso("PROVEEDORES", "editar")]
        public async Task<IActionResult> Edit(int id)
        {
            SetAuthHeader();
            var response = await _httpClient.GetAsync($"api/proveedores/{id}");
            if (!response.IsSuccessStatusCode) return NotFound();

            var proveedor = await response.Content.ReadFromJsonAsync<ProveedorResponseDto>();
            var dto = new ProveedorRequestDto(
                proveedor!.Nombre,
                proveedor.RFC,
                proveedor.Contacto ?? "",
                proveedor.Telefono ?? "",
                proveedor.Activo
            );
            ViewBag.Id = id;
            return View(dto);
        }

        // POST: Proveedores/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("PROVEEDORES", "editar")]
        public async Task<IActionResult> Edit(int id, ProveedorRequestDto proveedorDto)
        {
            if (ModelState.IsValid)
            {
                SetAuthHeader();
                var response = await _httpClient.PutAsJsonAsync($"api/proveedores/{id}", proveedorDto);
                if (response.IsSuccessStatusCode)
                {
                    TempData["Mensaje"] = "Proveedor actualizado exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", "Error al actualizar");
            }
            ViewBag.Id = id;
            return View(proveedorDto);
        }

        // GET: Proveedores/Delete/5
        [RequierePermiso("PROVEEDORES", "eliminar")]
        public async Task<IActionResult> Delete(int id)
        {
            SetAuthHeader();
            var response = await _httpClient.GetAsync($"api/proveedores/{id}");
            if (!response.IsSuccessStatusCode) return NotFound();

            var proveedor = await response.Content.ReadFromJsonAsync<ProveedorResponseDto>();
            return View(proveedor);
        }

        // POST: Proveedores/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [RequierePermiso("PROVEEDORES", "eliminar")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            SetAuthHeader();
            var response = await _httpClient.DeleteAsync($"api/proveedores/{id}");
            if (response.IsSuccessStatusCode)
                TempData["Mensaje"] = "Proveedor eliminado";
            else
                TempData["Error"] = "No se pudo eliminar el proveedor";

            return RedirectToAction(nameof(Index));
        }
    }
}