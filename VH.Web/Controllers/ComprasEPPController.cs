using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VH.Services.DTOs;
using VH.Web.Filters;

namespace VH.Web.Controllers
{
    [Authorize]
    [RequierePermiso("COMPRAS_EPP", "ver")]
    public class ComprasEPPController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ComprasEPPController> _logger;

        public ComprasEPPController(IHttpClientFactory httpClientFactory, ILogger<ComprasEPPController> logger)
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

        // GET: ComprasEPP
        public async Task<IActionResult> Index(int? idMaterial, int? idProveedor, int? idAlmacen)
        {
            SetAuthHeader();
            try
            {
                var url = "api/comprasepp";
                var queryParams = new List<string>();

                if (idMaterial.HasValue) queryParams.Add($"idMaterial={idMaterial}");
                if (idProveedor.HasValue) queryParams.Add($"idProveedor={idProveedor}");
                if (idAlmacen.HasValue) queryParams.Add($"idAlmacen={idAlmacen}");

                if (queryParams.Any()) url += "?" + string.Join("&", queryParams);

                var response = await _httpClient.GetAsync(url);
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    return RedirectToAction("Login", "Account");

                response.EnsureSuccessStatusCode();
                var compras = await response.Content.ReadFromJsonAsync<IEnumerable<CompraEPPResponseDto>>();

                await CargarFiltrosEnViewBag();
                ViewBag.FiltroMaterial = idMaterial;
                ViewBag.FiltroProveedor = idProveedor;
                ViewBag.FiltroAlmacen = idAlmacen;

                return View(compras);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar compras EPP");
                ViewBag.ErrorMessage = "Error al cargar las compras";
                return View(new List<CompraEPPResponseDto>());
            }
        }

        // GET: ComprasEPP/Create
        [RequierePermiso("COMPRAS_EPP", "crear")]
        public async Task<IActionResult> Create()
        {
            SetAuthHeader();
            await CargarListasEnViewBag();
            return View();
        }

        // POST: ComprasEPP/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("COMPRAS_EPP", "crear")]
        public async Task<IActionResult> Create(CompraEPPRequestDto dto)
        {
            if (ModelState.IsValid)
            {
                SetAuthHeader();
                try
                {
                    var response = await _httpClient.PostAsJsonAsync("api/comprasepp", dto);
                    if (response.IsSuccessStatusCode)
                    {
                        TempData["Mensaje"] = "Compra registrada exitosamente";
                        return RedirectToAction(nameof(Index));
                    }
                    var error = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", error);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al crear compra");
                    ModelState.AddModelError("", "Error al registrar la compra");
                }
            }
            await CargarListasEnViewBag();
            return View(dto);
        }

        // GET: ComprasEPP/Details/5
        public async Task<IActionResult> Details(int id)
        {
            SetAuthHeader();
            var response = await _httpClient.GetAsync($"api/comprasepp/{id}");
            if (!response.IsSuccessStatusCode) return NotFound();

            var compra = await response.Content.ReadFromJsonAsync<CompraEPPResponseDto>();
            return View(compra);
        }

        // GET: ComprasEPP/Edit/5
        [RequierePermiso("COMPRAS_EPP", "editar")]
        public async Task<IActionResult> Edit(int id)
        {
            SetAuthHeader();
            var response = await _httpClient.GetAsync($"api/comprasepp/{id}");
            if (!response.IsSuccessStatusCode) return NotFound();

            var compra = await response.Content.ReadFromJsonAsync<CompraEPPResponseDto>();
            await CargarListasEnViewBag();
            ViewBag.IdCompra = id;
            return View(compra);
        }

        // POST: ComprasEPP/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("COMPRAS_EPP", "editar")]
        public async Task<IActionResult> Edit(int id, CompraEPPRequestDto dto)
        {
            if (ModelState.IsValid)
            {
                SetAuthHeader();
                var response = await _httpClient.PutAsJsonAsync($"api/comprasepp/{id}", dto);
                if (response.IsSuccessStatusCode)
                {
                    TempData["Mensaje"] = "Compra actualizada exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", "Error al actualizar");
            }
            await CargarListasEnViewBag();
            ViewBag.IdCompra = id;
            return View(dto);
        }

        // POST: ComprasEPP/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("COMPRAS_EPP", "eliminar")]
        public async Task<IActionResult> Delete(int id)
        {
            SetAuthHeader();
            var response = await _httpClient.DeleteAsync($"api/comprasepp/{id}");
            if (response.IsSuccessStatusCode)
                TempData["Mensaje"] = "Compra eliminada";
            else
                TempData["Error"] = "No se pudo eliminar la compra";

            return RedirectToAction(nameof(Index));
        }

        private async Task CargarListasEnViewBag()
        {
            try
            {
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

                var proveedoresResponse = await _httpClient.GetAsync("api/proveedores");
                if (proveedoresResponse.IsSuccessStatusCode)
                {
                    var proveedores = await proveedoresResponse.Content.ReadFromJsonAsync<IEnumerable<ProveedorResponseDto>>();
                    ViewBag.Proveedores = new SelectList(proveedores, "IdProveedor", "Nombre");
                }
                else
                {
                    ViewBag.Proveedores = new SelectList(new List<ProveedorResponseDto>(), "IdProveedor", "Nombre");
                }

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
            }
            catch
            {
                ViewBag.Materiales = new SelectList(new List<MaterialEPPResponseDto>(), "IdMaterial", "Nombre");
                ViewBag.Proveedores = new SelectList(new List<ProveedorResponseDto>(), "IdProveedor", "Nombre");
                ViewBag.Almacenes = new SelectList(new List<AlmacenResponseDto>(), "IdAlmacen", "Nombre");
            }
        }

        private async Task CargarFiltrosEnViewBag()
        {
            await CargarListasEnViewBag();
        }
    }
}