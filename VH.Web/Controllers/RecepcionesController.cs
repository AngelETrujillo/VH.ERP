using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;
using VH.Services.DTOs;
using VH.Web.Filters;

namespace VH.Web.Controllers
{
    /// <summary>
    /// La puerta del almacén: lo que viene en camino, lo que se cuenta al llegar
    /// y a quién le toca lo que acaba de bajar del camión.
    /// </summary>
    [Authorize]
    [RequierePermiso("RECEPCIONES", "ver")]
    public class RecepcionesController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<RecepcionesController> _logger;

        public RecepcionesController(IHttpClientFactory httpClientFactory, ILogger<RecepcionesController> logger)
        {
            _httpClient = httpClientFactory.CreateClient("ApiERP");
            _logger = logger;
        }

        // GET: Recepciones — la bandeja de lo que viene en camino
        public async Task<IActionResult> Index(int? idAlmacen)
        {
            await CargarAlmacenesEnViewBag();
            ViewBag.FiltroAlmacen = idAlmacen;

            try
            {
                var url = "api/recepciones/porrecibir";
                if (idAlmacen.HasValue) url += $"?idAlmacen={idAlmacen}";

                var response = await _httpClient.GetAsync(url);
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    return RedirectToAction("Login", "Account");

                response.EnsureSuccessStatusCode();
                var pendientes = await response.Content.ReadFromJsonAsync<List<OrdenPorRecibirDto>>();

                return View(pendientes ?? new List<OrdenPorRecibirDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar las órdenes por recibir");
                ViewBag.ErrorMessage = "Error al cargar las órdenes por recibir";
                return View(new List<OrdenPorRecibirDto>());
            }
        }

        // GET: Recepciones/Recibir?idOrdenCompra=1&idAlmacen=2
        public async Task<IActionResult> Recibir(int idOrdenCompra, int idAlmacen)
        {
            try
            {
                var response = await _httpClient.GetAsync(
                    $"api/recepciones/preparacion?idOrdenCompra={idOrdenCompra}&idAlmacen={idAlmacen}");

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    return RedirectToAction("Login", "Account");

                if (!response.IsSuccessStatusCode)
                {
                    TempData["Error"] = ExtraerMensaje(await response.Content.ReadAsStringAsync());
                    return RedirectToAction(nameof(Index));
                }

                var preparacion = await response.Content.ReadFromJsonAsync<PreparacionRecepcionDto>();
                if (preparacion == null) return NotFound();

                return View(preparacion);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al preparar la recepción");
                TempData["Error"] = "Error al preparar la recepción";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Recepciones/Recibir
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("RECEPCIONES", "crear")]
        public async Task<IActionResult> Recibir(RecibirOrdenRequestDto dto)
        {
            // Los renglones que el almacenista no marcó no se reciben: llegan con
            // cantidad cero y aquí se descartan, para no crear lotes vacíos.
            var lineas = dto.Lineas?.Where(l => l.CantidadRecibida > 0).ToList() ?? new();

            if (lineas.Count == 0)
            {
                TempData["Error"] = "Indique la cantidad recibida de al menos un material.";
                return RedirectToAction(nameof(Recibir),
                    new { idOrdenCompra = dto.IdOrdenCompra, idAlmacen = dto.IdAlmacen });
            }

            var payload = dto with { Lineas = lineas };

            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/recepciones", payload);

                if (response.IsSuccessStatusCode)
                {
                    var resultado = await response.Content.ReadFromJsonAsync<RecibirResultado>();

                    TempData["Mensaje"] =
                        $"Recepción {resultado?.Recepcion?.Folio} registrada: " +
                        $"{resultado?.Recepcion?.TotalAceptado:0.##} pieza(s) entraron al almacén.";

                    if (resultado?.Avisos is { Count: > 0 })
                        TempData["Avisos"] = JsonSerializer.Serialize(resultado.Avisos);

                    return RedirectToAction(nameof(Details), new { id = resultado?.Recepcion?.IdRecepcion });
                }

                TempData["Error"] = ExtraerMensaje(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar la recepción");
                TempData["Error"] = "Error al registrar la recepción";
            }

            return RedirectToAction(nameof(Recibir),
                new { idOrdenCompra = dto.IdOrdenCompra, idAlmacen = dto.IdAlmacen });
        }

        // GET: Recepciones/Historial
        public async Task<IActionResult> Historial()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/recepciones");
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    return RedirectToAction("Login", "Account");

                response.EnsureSuccessStatusCode();
                var recepciones = await response.Content.ReadFromJsonAsync<List<RecepcionCompraResponseDto>>();

                return View(recepciones ?? new List<RecepcionCompraResponseDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar el historial de recepciones");
                ViewBag.ErrorMessage = "Error al cargar el historial de recepciones";
                return View(new List<RecepcionCompraResponseDto>());
            }
        }

        // GET: Recepciones/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var response = await _httpClient.GetAsync($"api/recepciones/{id}");
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return RedirectToAction("Login", "Account");

            if (!response.IsSuccessStatusCode) return NotFound();

            var recepcion = await response.Content.ReadFromJsonAsync<RecepcionCompraResponseDto>();
            return View(recepcion);
        }

        private sealed class RecibirResultado
        {
            public RecepcionCompraResponseDto? Recepcion { get; set; }
            public List<string> Avisos { get; set; } = new();
        }

        private static string ExtraerMensaje(string cuerpo)
        {
            try
            {
                using var doc = JsonDocument.Parse(cuerpo);
                if (doc.RootElement.TryGetProperty("message", out var mensaje))
                    return mensaje.GetString() ?? cuerpo;
            }
            catch
            {
                // El cuerpo no era JSON; se muestra tal cual.
            }

            return cuerpo;
        }

        private async Task CargarAlmacenesEnViewBag()
        {
            try
            {
                var almacenes = await _httpClient.GetFromJsonAsync<IEnumerable<AlmacenResponseDto>>("api/almacenes");
                ViewBag.Almacenes = almacenes?
                    .OrderBy(a => a.Nombre)
                    .Select(a => new SelectListItem
                    {
                        Value = a.IdAlmacen.ToString(),
                        Text = a.Nombre
                    }).ToList() ?? new List<SelectListItem>();
            }
            catch
            {
                ViewBag.Almacenes = new List<SelectListItem>();
            }
        }
    }
}
