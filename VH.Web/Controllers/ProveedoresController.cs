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
                true
            );
            ViewBag.IdProveedor = id;
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
            ViewBag.IdProveedor = id;
            return View(proveedorDto);
        }

        // POST: Proveedores/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("PROVEEDORES", "eliminar")]
        public async Task<IActionResult> Delete(int id)
        {
            SetAuthHeader();
            await _httpClient.DeleteAsync($"api/proveedores/{id}");
            TempData["Mensaje"] = "Proveedor eliminado";
            return RedirectToAction(nameof(Index));
        }
    }
}