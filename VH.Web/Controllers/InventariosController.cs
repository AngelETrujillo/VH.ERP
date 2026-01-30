using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VH.Services.DTOs;
using VH.Web.Filters;

namespace VH.Web.Controllers
{
    [Authorize]
    [RequierePermiso("INVENTARIOS", "ver")]
    public class InventariosController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<InventariosController> _logger;

        public InventariosController(IHttpClientFactory httpClientFactory, ILogger<InventariosController> logger)
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

        // GET: Inventarios
        public async Task<IActionResult> Index()
        {
            SetAuthHeader();
            try
            {
                var response = await _httpClient.GetAsync("api/inventarios");
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    return RedirectToAction("Login", "Account");

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
        [RequierePermiso("INVENTARIOS", "crear")]
        public async Task<IActionResult> Create()
        {
            SetAuthHeader();
            await CargarListasEnViewBag();
            return View();
        }

        // POST: Inventarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("INVENTARIOS", "crear")]
        public async Task<IActionResult> Create(InventarioRequestDto dto)
        {
            if (ModelState.IsValid)
            {
                SetAuthHeader();
                try
                {
                    var response = await _httpClient.PostAsJsonAsync("api/inventarios", dto);
                    if (response.IsSuccessStatusCode)
                    {
                        TempData["Mensaje"] = "Inventario creado exitosamente";
                        return RedirectToAction(nameof(Index));
                    }
                    var error = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", error);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al crear inventario");
                    ModelState.AddModelError("", "Error al crear el inventario");
                }
            }
            await CargarListasEnViewBag();
            return View(dto);
        }

        // GET: Inventarios/Edit/5
        [RequierePermiso("INVENTARIOS", "editar")]
        public async Task<IActionResult> Edit(int id)
        {
            SetAuthHeader();
            var response = await _httpClient.GetAsync($"api/inventarios/{id}");
            if (!response.IsSuccessStatusCode) return NotFound();

            var inventario = await response.Content.ReadFromJsonAsync<InventarioResponseDto>();

            ViewBag.IdInventario = id;
            ViewBag.NombreAlmacen = inventario?.NombreAlmacen;
            ViewBag.NombreMaterial = inventario?.NombreMaterial;

            var dto = new InventarioRequestDto(
                inventario!.IdAlmacen,
                inventario.IdMaterial,
                inventario.StockMinimo,
                inventario.StockMaximo,
                inventario.UbicacionPasillo
            );

            return View(dto);
        }

        // POST: Inventarios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("INVENTARIOS", "editar")]
        public async Task<IActionResult> Edit(int id, InventarioRequestDto dto)
        {
            if (ModelState.IsValid)
            {
                SetAuthHeader();
                var response = await _httpClient.PutAsJsonAsync($"api/inventarios/{id}", dto);
                if (response.IsSuccessStatusCode)
                {
                    TempData["Mensaje"] = "Inventario actualizado exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", "Error al actualizar");
            }
            ViewBag.IdInventario = id;
            return View(dto);
        }

        // POST: Inventarios/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("INVENTARIOS", "eliminar")]
        public async Task<IActionResult> Delete(int id)
        {
            SetAuthHeader();
            var response = await _httpClient.DeleteAsync($"api/inventarios/{id}");
            if (response.IsSuccessStatusCode)
                TempData["Mensaje"] = "Inventario eliminado";
            else
                TempData["Error"] = "No se pudo eliminar el inventario";

            return RedirectToAction(nameof(Index));
        }

        private async Task CargarListasEnViewBag()
        {
            try
            {
                var almacenesResponse = await _httpClient.GetAsync("api/almacenes");
                if (almacenesResponse.IsSuccessStatusCode)
                {
                    var almacenes = await almacenesResponse.Content.ReadFromJsonAsync<IEnumerable<AlmacenResponseDto>>();
                    ViewBag.Almacenes = new SelectList(almacenes, "IdAlmacen", "Nombre");
                }
                else
                {
                    ViewBag.Almacenes = new SelectList(new List<AlmacenResponseDto>(), "IdAlmacen", "Nombre");
                }

                var materialesResponse = await _httpClient.GetAsync("api/materiales");
                if (materialesResponse.IsSuccessStatusCode)
                {
                    var materiales = await materialesResponse.Content.ReadFromJsonAsync<IEnumerable<MaterialEPPResponseDto>>();
                    ViewBag.Materiales = new SelectList(materiales, "IdMaterial", "Nombre");
                }
                else
                {
                    ViewBag.Materiales = new SelectList(new List<MaterialEPPResponseDto>(), "IdMaterial", "Nombre");
                }
            }
            catch
            {
                ViewBag.Almacenes = new SelectList(new List<AlmacenResponseDto>(), "IdAlmacen", "Nombre");
                ViewBag.Materiales = new SelectList(new List<MaterialEPPResponseDto>(), "IdMaterial", "Nombre");
            }
        }
    }
}