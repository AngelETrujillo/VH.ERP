using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;
using VH.Services.DTOs;
using VH.Web.Filters;

namespace VH.Web.Controllers
{
    /// <summary>
    /// Lo que se prestó y sigue afuera, y su vuelta al almacén.
    /// </summary>
    [Authorize]
    [RequierePermiso("DEVOLUCIONES", "ver")]
    public class DevolucionesController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<DevolucionesController> _logger;

        public DevolucionesController(IHttpClientFactory httpClientFactory, ILogger<DevolucionesController> logger)
        {
            _httpClient = httpClientFactory.CreateClient("ApiERP");
            _logger = logger;
        }

        // GET: Devoluciones — quién tiene qué
        public async Task<IActionResult> Index(int? idAlmacen)
        {
            await CargarAlmacenesEnViewBag();
            ViewBag.FiltroAlmacen = idAlmacen;

            try
            {
                var url = "api/devoluciones/prestados";
                if (idAlmacen.HasValue) url += $"?idAlmacen={idAlmacen}";

                var response = await _httpClient.GetAsync(url);
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    return RedirectToAction("Login", "Account");

                response.EnsureSuccessStatusCode();
                var prestados = await response.Content.ReadFromJsonAsync<List<PrestamoDto>>();

                return View(prestados ?? new List<PrestamoDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar lo prestado");
                ViewBag.ErrorMessage = "Error al cargar el material prestado";
                return View(new List<PrestamoDto>());
            }
        }

        // POST: Devoluciones/Registrar
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("DEVOLUCIONES", "crear")]
        public async Task<IActionResult> Registrar(RegistrarDevolucionRequestDto dto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/devoluciones", dto);

                if (response.IsSuccessStatusCode)
                {
                    var resultado = await response.Content.ReadFromJsonAsync<DevolucionResultado>();

                    TempData["Mensaje"] = "Devolución registrada.";

                    if (!string.IsNullOrWhiteSpace(resultado?.Aviso))
                        TempData["WarningMessage"] = resultado.Aviso;
                }
                else
                {
                    TempData["Error"] = ExtraerMensaje(await response.Content.ReadAsStringAsync());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar la devolución");
                TempData["Error"] = "Error al registrar la devolución";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Devoluciones/Historial
        public async Task<IActionResult> Historial(
            int pagina = 1, int tamano = ConsultaPaginada.TamanoPorOmision, string? buscar = null)
        {
            var consulta = new ConsultaPaginada { Pagina = pagina, Tamano = tamano, Buscar = buscar };

            try
            {
                var partes = new List<string> { $"pagina={consulta.Pagina}", $"tamano={consulta.Tamano}" };
                if (consulta.HayBusqueda) partes.Add($"buscar={Uri.EscapeDataString(consulta.TextoLimpio!)}");

                var url = "api/devoluciones/paginado?" + string.Join("&", partes);

                var response = await _httpClient.GetAsync(url);
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    return RedirectToAction("Login", "Account");

                response.EnsureSuccessStatusCode();
                var pag = await response.Content.ReadFromJsonAsync<ResultadoPaginado<DevolucionResponseDto>>();

                return View(pag ?? ResultadoPaginado<DevolucionResponseDto>.Ninguno(consulta));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar el historial de devoluciones");
                ViewBag.ErrorMessage = "Error al cargar el historial";
                return View(ResultadoPaginado<DevolucionResponseDto>.Ninguno(consulta));
            }
        }

        private sealed class DevolucionResultado
        {
            public DevolucionResponseDto? Devolucion { get; set; }
            public string? Aviso { get; set; }
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
                    .Select(a => new SelectListItem { Value = a.IdAlmacen.ToString(), Text = a.Nombre })
                    .ToList() ?? new List<SelectListItem>();
            }
            catch
            {
                ViewBag.Almacenes = new List<SelectListItem>();
            }
        }
    }
}
