using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VH.Services.DTOs;
using VH.Services.Entities;
using VH.Web.Filters;

namespace VH.Web.Controllers
{
    [Authorize]
    [RequierePermiso("ORDENES_COMPRA", "ver")]
    public class OrdenesCompraController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<OrdenesCompraController> _logger;

        public OrdenesCompraController(IHttpClientFactory httpClientFactory, ILogger<OrdenesCompraController> logger)
        {
            _httpClient = httpClientFactory.CreateClient("ApiERP");
            _logger = logger;
        }

        // GET: OrdenesCompra
        public async Task<IActionResult> Index()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/ordenescompra");
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    return RedirectToAction("Login", "Account");

                response.EnsureSuccessStatusCode();
                var ordenes = await response.Content.ReadFromJsonAsync<IEnumerable<OrdenCompraResponseDto>>();
                return View(ordenes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar órdenes de compra");
                ViewBag.ErrorMessage = "Error al cargar las órdenes de compra";
                return View(new List<OrdenCompraResponseDto>());
            }
        }

        // GET: OrdenesCompra/Faltantes
        // La pantalla donde vive el trabajo del Comprador.
        public async Task<IActionResult> Faltantes(int? idAlmacen)
        {
            try
            {
                var url = "api/ordenescompra/faltantes";
                if (idAlmacen.HasValue) url += $"?idAlmacen={idAlmacen}";

                var response = await _httpClient.GetAsync(url);
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    return RedirectToAction("Login", "Account");

                response.EnsureSuccessStatusCode();
                var faltantes = await response.Content.ReadFromJsonAsync<IEnumerable<FaltanteDto>>();

                await CargarListasEnViewBag();
                ViewBag.FiltroAlmacen = idAlmacen;

                return View(faltantes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar faltantes");
                ViewBag.ErrorMessage = "Error al calcular los faltantes";
                await CargarListasEnViewBag();
                return View(new List<FaltanteDto>());
            }
        }

        // POST: OrdenesCompra/Generar
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("ORDENES_COMPRA", "crear")]
        public async Task<IActionResult> Generar(GenerarOrdenCompraRequestDto dto)
        {
            if (dto.Lineas == null || dto.Lineas.Count == 0)
            {
                TempData["Error"] = "Seleccione al menos un material para la orden";
                return RedirectToAction(nameof(Faltantes));
            }

            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/ordenescompra", dto);
                if (response.IsSuccessStatusCode)
                {
                    var resultado = await response.Content.ReadFromJsonAsync<GenerarResultado>();
                    var orden = resultado?.Orden;

                    TempData["Mensaje"] = $"Orden {orden?.Folio} emitida. Los materiales salen de la bandeja de faltantes.";

                    // Si la orden se quedó corta para alguien, el comprador tiene
                    // que enterarse ahora, no cuando llegue el material.
                    if (resultado?.Avisos is { Count: > 0 })
                        TempData["Avisos"] = System.Text.Json.JsonSerializer.Serialize(resultado.Avisos);

                    return RedirectToAction(nameof(Details), new { id = orden?.IdOrdenCompra });
                }

                TempData["Error"] = await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar orden de compra");
                TempData["Error"] = "Error al emitir la orden de compra";
            }

            return RedirectToAction(nameof(Faltantes));
        }

        // GET: OrdenesCompra/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var response = await _httpClient.GetAsync($"api/ordenescompra/{id}");
            if (!response.IsSuccessStatusCode) return NotFound();

            var orden = await response.Content.ReadFromJsonAsync<OrdenCompraResponseDto>();
            return View(orden);
        }

        // POST: OrdenesCompra/Cancelar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("ORDENES_COMPRA", "eliminar")]
        public async Task<IActionResult> Cancelar(int id, string motivo)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(
                    $"api/ordenescompra/{id}/cancelar", new CancelarOrdenCompraRequestDto(motivo ?? ""));

                if (response.IsSuccessStatusCode)
                    TempData["Mensaje"] = "Orden cancelada. Los materiales vuelven a la bandeja de faltantes.";
                else
                    TempData["Error"] = await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cancelar orden");
                TempData["Error"] = "Error al cancelar la orden";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        private sealed class GenerarResultado
        {
            public OrdenCompraResponseDto? Orden { get; set; }
            public List<string> Avisos { get; set; } = new();
        }

        private async Task CargarListasEnViewBag()
        {
            try
            {
                var almacenes = await _httpClient.GetFromJsonAsync<IEnumerable<AlmacenResponseDto>>("api/almacenes");
                ViewBag.Almacenes = almacenes?.Select(a => new SelectListItem
                {
                    Value = a.IdAlmacen.ToString(),
                    Text = a.Nombre
                }).ToList() ?? new List<SelectListItem>();

                var proveedores = await _httpClient.GetFromJsonAsync<IEnumerable<ProveedorResponseDto>>("api/proveedores");
                ViewBag.Proveedores = proveedores?.Select(p => new SelectListItem
                {
                    Value = p.IdProveedor.ToString(),
                    Text = p.Nombre
                }).ToList() ?? new List<SelectListItem>();
            }
            catch
            {
                ViewBag.Almacenes = new List<SelectListItem>();
                ViewBag.Proveedores = new List<SelectListItem>();
            }
        }
    }
}
