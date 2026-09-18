using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VH.Services.DTOs;
using VH.Web.Filters;

namespace VH.Web.Controllers
{
    /// <summary>
    /// El libro del almacén, visto desde la oficina: la historia de un material en
    /// un almacén, el saldo que tenía en una fecha y las correcciones que se le
    /// hacen (conteo, traspaso, merma).
    /// </summary>
    [Authorize]
    [RequierePermiso("KARDEX", "ver")]
    public class KardexController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<KardexController> _logger;

        public KardexController(IHttpClientFactory httpClientFactory, ILogger<KardexController> logger)
        {
            _httpClient = httpClientFactory.CreateClient("ApiERP");
            _logger = logger;
        }

        // GET: Kardex?idMaterial=1&idAlmacen=1
        public async Task<IActionResult> Index(int? idMaterial, int? idAlmacen, DateTime? desde, DateTime? hasta)
        {
            await CargarListasEnViewBag();

            ViewBag.IdMaterial = idMaterial;
            ViewBag.IdAlmacen = idAlmacen;
            ViewBag.Desde = desde;
            ViewBag.Hasta = hasta;

            if (!idMaterial.HasValue || !idAlmacen.HasValue)
                return View(new List<MovimientoInventarioResponseDto>());

            try
            {
                var url = $"api/kardex?idMaterial={idMaterial}&idAlmacen={idAlmacen}";
                if (desde.HasValue) url += $"&desde={desde.Value:yyyy-MM-dd}";
                if (hasta.HasValue) url += $"&hasta={hasta.Value:yyyy-MM-dd}T23:59:59";

                var response = await _httpClient.GetAsync(url);
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    return RedirectToAction("Login", "Account");

                response.EnsureSuccessStatusCode();
                var movimientos = await response.Content.ReadFromJsonAsync<List<MovimientoInventarioResponseDto>>();

                return View(movimientos ?? new List<MovimientoInventarioResponseDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar el kardex");
                ViewBag.ErrorMessage = "Error al cargar el kardex";
                return View(new List<MovimientoInventarioResponseDto>());
            }
        }

        // GET: Kardex/Descuadres
        // La prueba de que la existencia y su historia no se separaron.
        public async Task<IActionResult> Descuadres()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/kardex/descuadres");
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    return RedirectToAction("Login", "Account");

                response.EnsureSuccessStatusCode();
                var descuadres = await response.Content.ReadFromJsonAsync<List<DescuadreDto>>();

                return View(descuadres ?? new List<DescuadreDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al reconciliar el inventario");
                ViewBag.ErrorMessage = "Error al reconciliar el inventario";
                return View(new List<DescuadreDto>());
            }
        }

        // POST: Kardex/Ajustar
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("KARDEX", "crear")]
        public async Task<IActionResult> Ajustar(int idMaterial, int idAlmacen, decimal existenciaContada, string motivo)
        {
            await EnviarAsync(
                "api/kardex/ajuste",
                new AjusteInventarioRequestDto(idMaterial, idAlmacen, existenciaContada, motivo ?? string.Empty),
                "Existencia ajustada al conteo físico.");

            return RedirectToAction(nameof(Index), new { idMaterial, idAlmacen });
        }

        // POST: Kardex/Traspasar
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("KARDEX", "crear")]
        public async Task<IActionResult> Traspasar(
            int idMaterial, int idAlmacenOrigen, int idAlmacenDestino, decimal cantidad, string motivo)
        {
            await EnviarAsync(
                "api/kardex/traspaso",
                new TraspasoRequestDto(idMaterial, idAlmacenOrigen, idAlmacenDestino, cantidad, motivo ?? string.Empty),
                "Traspaso registrado en los dos almacenes.");

            return RedirectToAction(nameof(Index), new { idMaterial, idAlmacen = idAlmacenOrigen });
        }

        // POST: Kardex/Merma
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("KARDEX", "crear")]
        public async Task<IActionResult> Merma(int idMaterial, int idAlmacen, decimal cantidad, string motivo)
        {
            await EnviarAsync(
                "api/kardex/merma",
                new MermaRequestDto(idMaterial, idAlmacen, cantidad, motivo ?? string.Empty),
                "Baja registrada.");

            return RedirectToAction(nameof(Index), new { idMaterial, idAlmacen });
        }

        /// <summary>
        /// Manda la operación al API y deja en TempData lo que hay que decirle al
        /// usuario. El error del API se muestra tal cual: ahí está el motivo real
        /// por el que no se pudo mover el material.
        /// </summary>
        private async Task EnviarAsync<T>(string url, T payload, string mensajeExito)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(url, payload);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Mensaje"] = mensajeExito;
                    return;
                }

                var cuerpo = await response.Content.ReadAsStringAsync();
                TempData["Error"] = ExtraerMensaje(cuerpo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar el movimiento de inventario");
                TempData["Error"] = "No se pudo registrar el movimiento.";
            }
        }

        private static string ExtraerMensaje(string cuerpo)
        {
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(cuerpo);
                if (doc.RootElement.TryGetProperty("message", out var mensaje))
                    return mensaje.GetString() ?? cuerpo;
            }
            catch
            {
                // El cuerpo no era JSON; se muestra tal cual.
            }

            return cuerpo;
        }

        private async Task CargarListasEnViewBag()
        {
            try
            {
                var materiales = await _httpClient.GetFromJsonAsync<IEnumerable<MaterialResponseDto>>("api/materiales");
                ViewBag.Materiales = materiales?
                    .OrderBy(m => m.Nombre)
                    .Select(m => new SelectListItem
                    {
                        Value = m.IdMaterial.ToString(),
                        Text = m.Nombre
                    }).ToList() ?? new List<SelectListItem>();

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
                ViewBag.Materiales = new List<SelectListItem>();
                ViewBag.Almacenes = new List<SelectListItem>();
            }
        }
    }
}
