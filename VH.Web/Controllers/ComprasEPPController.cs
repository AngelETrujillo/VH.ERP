using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VH.Services.DTOs;

namespace VH.Web.Controllers
{
    public class ComprasEPPController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ComprasEPPController> _logger;

        public ComprasEPPController(IHttpClientFactory httpClientFactory, ILogger<ComprasEPPController> logger)
        {
            _httpClient = httpClientFactory.CreateClient("ApiERP");
            _logger = logger;
        }

        // GET: ComprasEPP
        public async Task<IActionResult> Index(int? idMaterial, int? idProveedor, int? idAlmacen)
        {
            try
            {
                var url = "api/comprasepp";
                var queryParams = new List<string>();

                if (idMaterial.HasValue) queryParams.Add($"idMaterial={idMaterial}");
                if (idProveedor.HasValue) queryParams.Add($"idProveedor={idProveedor}");
                if (idAlmacen.HasValue) queryParams.Add($"idAlmacen={idAlmacen}");

                if (queryParams.Any()) url += "?" + string.Join("&", queryParams);

                var response = await _httpClient.GetAsync(url);
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
        public async Task<IActionResult> Create()
        {
            await CargarListasEnViewBag();
            return View();
        }

        // POST: ComprasEPP/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CompraEPPRequestDto dto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var response = await _httpClient.PostAsJsonAsync("api/comprasepp", dto);
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadFromJsonAsync<ApiResponseWithAlert<CompraEPPResponseDto>>();

                        if (!string.IsNullOrEmpty(result?.Alerta))
                        {
                            TempData["WarningMessage"] = result.Alerta;
                        }
                        else
                        {
                            TempData["SuccessMessage"] = "Compra registrada exitosamente";
                        }
                        return RedirectToAction(nameof(Index));
                    }
                    var error = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", $"Error: {error}");
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

        // GET: ComprasEPP/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var compra = await _httpClient.GetFromJsonAsync<CompraEPPResponseDto>($"api/comprasepp/{id}");
                if (compra == null) return NotFound();
                return View(compra);
            }
            catch
            {
                return NotFound();
            }
        }

        // GET: ComprasEPP/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var compra = await _httpClient.GetFromJsonAsync<CompraEPPResponseDto>($"api/comprasepp/{id}");
                if (compra == null) return NotFound();

                var dto = new CompraEPPRequestDto(
                    compra.IdMaterial,
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
            catch
            {
                return NotFound();
            }
        }

        // POST: ComprasEPP/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CompraEPPRequestDto dto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var response = await _httpClient.PutAsJsonAsync($"api/comprasepp/{id}", dto);
                    if (response.IsSuccessStatusCode)
                    {
                        TempData["SuccessMessage"] = "Compra actualizada exitosamente";
                        return RedirectToAction(nameof(Index));
                    }
                    var error = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", $"Error: {error}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al actualizar compra EPP");
                    ModelState.AddModelError("", "Error al actualizar");
                }
            }
            await CargarListasEnViewBag();
            ViewBag.Id = id;
            return View(dto);
        }

        // GET: ComprasEPP/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var compra = await _httpClient.GetFromJsonAsync<CompraEPPResponseDto>($"api/comprasepp/{id}");
                if (compra == null) return NotFound();
                return View(compra);
            }
            catch
            {
                return NotFound();
            }
        }

        // POST: ComprasEPP/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/comprasepp/{id}");
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Compra eliminada exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                var error = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"No se pudo eliminar: {error}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar compra EPP");
                TempData["ErrorMessage"] = "Error al eliminar";
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: ComprasEPP/HistorialPrecios/5
        public async Task<IActionResult> HistorialPrecios(int idMaterial, int? idProveedor)
        {
            try
            {
                var url = $"api/comprasepp/historial-precios/{idMaterial}";
                if (idProveedor.HasValue) url += $"?idProveedor={idProveedor}";

                var historial = await _httpClient.GetFromJsonAsync<IEnumerable<CompraEPPResponseDto>>(url);

                var material = await _httpClient.GetFromJsonAsync<MaterialEPPResponseDto>($"api/materiales/{idMaterial}");
                ViewBag.NombreMaterial = material?.Nombre ?? "Material";
                ViewBag.IdMaterial = idMaterial;

                await CargarProveedoresEnViewBag();
                ViewBag.FiltroProveedor = idProveedor;

                return View(historial);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar historial de precios");
                TempData["ErrorMessage"] = "Error al cargar el historial";
                return RedirectToAction(nameof(Index));
            }
        }

        private async Task CargarListasEnViewBag()
        {
            try
            {
                var materiales = await _httpClient.GetFromJsonAsync<IEnumerable<MaterialEPPResponseDto>>("api/materiales");
                ViewBag.Materiales = materiales?.Where(m => m.Activo).Select(m => new SelectListItem
                {
                    Value = m.IdMaterial.ToString(),
                    Text = $"{m.Nombre} ({m.AbreviaturaUnidadMedida})"
                }).ToList() ?? new List<SelectListItem>();

                var proveedores = await _httpClient.GetFromJsonAsync<IEnumerable<ProveedorResponseDto>>("api/proveedores");
                ViewBag.Proveedores = proveedores?.Where(p => p.Activo).Select(p => new SelectListItem
                {
                    Value = p.IdProveedor.ToString(),
                    Text = p.Nombre
                }).ToList() ?? new List<SelectListItem>();

                var almacenes = await _httpClient.GetFromJsonAsync<IEnumerable<AlmacenResponseDto>>("api/almacenes");
                ViewBag.Almacenes = almacenes?.Where(a => a.Activo).Select(a => new SelectListItem
                {
                    Value = a.IdAlmacen.ToString(),
                    Text = $"{a.Nombre} ({a.NombreProyecto})"
                }).ToList() ?? new List<SelectListItem>();
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
                var proveedores = await _httpClient.GetFromJsonAsync<IEnumerable<ProveedorResponseDto>>("api/proveedores");
                ViewBag.ProveedoresFiltro = proveedores?.Select(p => new SelectListItem
                {
                    Value = p.IdProveedor.ToString(),
                    Text = p.Nombre
                }).ToList() ?? new List<SelectListItem>();
            }
            catch
            {
                ViewBag.ProveedoresFiltro = new List<SelectListItem>();
            }
        }
    }

    // Clase auxiliar para deserializar respuestas con alerta
    public class ApiResponseWithAlert<T>
    {
        public T? Data { get; set; }
        public string? Alerta { get; set; }
    }
}