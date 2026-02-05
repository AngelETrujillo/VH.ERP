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

        // GET: ComprasEPP/Details/5
        public async Task<IActionResult> Details(int id)
        {
            SetAuthHeader();
            var response = await _httpClient.GetAsync($"api/comprasepp/{id}");
            if (!response.IsSuccessStatusCode) return NotFound();

            var compra = await response.Content.ReadFromJsonAsync<CompraEPPResponseDto>();
            return View(compra);
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
                    _logger.LogError(ex, "Error al crear compra EPP");
                    ModelState.AddModelError("", "Error al registrar la compra");
                }
            }
            await CargarListasEnViewBag();
            return View(dto);
        }

        // GET: ComprasEPP/Edit/5
        [RequierePermiso("COMPRAS_EPP", "editar")]
        public async Task<IActionResult> Edit(int id)
        {
            SetAuthHeader();
            var response = await _httpClient.GetAsync($"api/comprasepp/{id}");
            if (!response.IsSuccessStatusCode) return NotFound();

            var compra = await response.Content.ReadFromJsonAsync<CompraEPPResponseDto>();
            var dto = new CompraEPPRequestDto(
                compra!.IdMaterial,
                compra.IdProveedor,
                compra.IdAlmacen,
                compra.FechaCompra,
                compra.CantidadComprada,
                compra.PrecioUnitario,
                compra.NumeroDocumento,
                compra.Observaciones
            );

            await CargarListasEnViewBag();
            ViewBag.Id = id;
            ViewBag.CantidadDisponible = compra.CantidadDisponible;
            return View(dto);
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
            ViewBag.Id = id;
            return View(dto);
        }

        // GET: ComprasEPP/Delete/5
        [RequierePermiso("COMPRAS_EPP", "eliminar")]
        public async Task<IActionResult> Delete(int id)
        {
            SetAuthHeader();
            var response = await _httpClient.GetAsync($"api/comprasepp/{id}");
            if (!response.IsSuccessStatusCode) return NotFound();

            var compra = await response.Content.ReadFromJsonAsync<CompraEPPResponseDto>();
            return View(compra);
        }

        // POST: ComprasEPP/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [RequierePermiso("COMPRAS_EPP", "eliminar")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            SetAuthHeader();
            var response = await _httpClient.DeleteAsync($"api/comprasepp/{id}");
            if (response.IsSuccessStatusCode)
                TempData["Mensaje"] = "Compra eliminada";
            else
                TempData["Error"] = "No se pudo eliminar la compra";

            return RedirectToAction(nameof(Index));
        }

        // GET: ComprasEPP/HistorialPrecios/5
        public async Task<IActionResult> HistorialPrecios(int idMaterial, int? idProveedor)
        {
            SetAuthHeader();
            try
            {
                var url = $"api/comprasepp/historial-precios/{idMaterial}";
                if (idProveedor.HasValue) url += $"?idProveedor={idProveedor}";

                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode) return RedirectToAction(nameof(Index));

                var historial = await response.Content.ReadFromJsonAsync<IEnumerable<CompraEPPResponseDto>>();

                var materialResponse = await _httpClient.GetAsync($"api/materiales/{idMaterial}");
                if (materialResponse.IsSuccessStatusCode)
                {
                    var material = await materialResponse.Content.ReadFromJsonAsync<MaterialEPPResponseDto>();
                    ViewBag.NombreMaterial = material?.Nombre ?? "Material";
                }
                else
                {
                    ViewBag.NombreMaterial = "Material";
                }

                ViewBag.IdMaterial = idMaterial;
                await CargarProveedoresEnViewBag();
                ViewBag.FiltroProveedor = idProveedor;

                return View(historial);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar historial de precios");
                TempData["Error"] = "Error al cargar el historial";
                return RedirectToAction(nameof(Index));
            }
        }

        private async Task CargarListasEnViewBag()
        {
            try
            {
                var materialesResponse = await _httpClient.GetAsync("api/materiales");
                if (materialesResponse.IsSuccessStatusCode)
                {
                    var materiales = await materialesResponse.Content.ReadFromJsonAsync<IEnumerable<MaterialEPPResponseDto>>();
                    ViewBag.Materiales = materiales?.Where(m => m.Activo).Select(m => new SelectListItem
                    {
                        Value = m.IdMaterial.ToString(),
                        Text = $"{m.Nombre} ({m.AbreviaturaUnidadMedida})"
                    }).ToList() ?? new List<SelectListItem>();
                }
                else
                {
                    ViewBag.Materiales = new List<SelectListItem>();
                }

                var proveedoresResponse = await _httpClient.GetAsync("api/proveedores");
                if (proveedoresResponse.IsSuccessStatusCode)
                {
                    var proveedores = await proveedoresResponse.Content.ReadFromJsonAsync<IEnumerable<ProveedorResponseDto>>();
                    ViewBag.Proveedores = proveedores?.Where(p => p.Activo).Select(p => new SelectListItem
                    {
                        Value = p.IdProveedor.ToString(),
                        Text = p.Nombre
                    }).ToList() ?? new List<SelectListItem>();
                }
                else
                {
                    ViewBag.Proveedores = new List<SelectListItem>();
                }

                var almacenesResponse = await _httpClient.GetAsync("api/almacenes");
                if (almacenesResponse.IsSuccessStatusCode)
                {
                    var almacenes = await almacenesResponse.Content.ReadFromJsonAsync<IEnumerable<AlmacenResponseDto>>();
                    ViewBag.Almacenes = almacenes?.Where(a => a.Activo).Select(a => new SelectListItem
                    {
                        Value = a.IdAlmacen.ToString(),
                        Text = $"{a.Nombre} ({a.NombreProyecto})"
                    }).ToList() ?? new List<SelectListItem>();
                }
                else
                {
                    ViewBag.Almacenes = new List<SelectListItem>();
                }
            }
            catch
            {
                ViewBag.Materiales = new List<SelectListItem>();
                ViewBag.Proveedores = new List<SelectListItem>();
                ViewBag.Almacenes = new List<SelectListItem>();
            }
        }

        private async Task CargarFiltrosEnViewBag()
        {
            await CargarListasEnViewBag();
        }

        private async Task CargarProveedoresEnViewBag()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/proveedores");
                if (response.IsSuccessStatusCode)
                {
                    var proveedores = await response.Content.ReadFromJsonAsync<IEnumerable<ProveedorResponseDto>>();
                    ViewBag.ProveedoresFiltro = proveedores?.Select(p => new SelectListItem
                    {
                        Value = p.IdProveedor.ToString(),
                        Text = p.Nombre
                    }).ToList() ?? new List<SelectListItem>();
                }
                else
                {
                    ViewBag.ProveedoresFiltro = new List<SelectListItem>();
                }
            }
            catch
            {
                ViewBag.ProveedoresFiltro = new List<SelectListItem>();
            }
        }
    }
}