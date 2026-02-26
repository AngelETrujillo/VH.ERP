using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VH.Services.DTOs.Analytics;
using VH.Services.Entities;
using VH.Web.Filters;

namespace VH.Web.Controllers
{
    [Authorize]
    [RequierePermiso("ALERTAS_CONSUMO", "ver")]
    public class AlertasConsumoController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AlertasConsumoController> _logger;

        public AlertasConsumoController(IHttpClientFactory httpClientFactory, ILogger<AlertasConsumoController> logger)
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

        public async Task<IActionResult> Index(
     int? idProyecto = null,
     int? idEmpleado = null,
     int? severidad = null,
     int? estado = null,
     bool soloPendientes = false)
        {
            SetAuthHeader();

            // Cargar filtros SIEMPRE primero (antes del try-catch de datos)
            await CargarFiltrosEnViewBag();
            ViewBag.FiltroProyecto = idProyecto;
            ViewBag.FiltroEmpleado = idEmpleado;
            ViewBag.FiltroSeveridad = severidad;
            ViewBag.FiltroEstado = estado;
            ViewBag.SoloPendientes = soloPendientes;

            try
            {
                var url = "api/alertasconsumo?";
                if (idProyecto.HasValue) url += $"idProyecto={idProyecto}&";
                if (idEmpleado.HasValue) url += $"idEmpleado={idEmpleado}&";
                if (severidad.HasValue) url += $"severidad={severidad}&";
                if (estado.HasValue) url += $"estado={estado}&";
                if (soloPendientes) url += "soloPendientes=true&";

                var alertas = await _httpClient.GetFromJsonAsync<IEnumerable<AlertaConsumoResponseDto>>(url);
                var resumen = await _httpClient.GetFromJsonAsync<ResumenAlertasDto>($"api/alertasconsumo/resumen?idProyecto={idProyecto}");

                ViewBag.Alertas = alertas ?? new List<AlertaConsumoResponseDto>();
                ViewBag.Resumen = resumen ?? new ResumenAlertasDto();

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar alertas");
                TempData["Error"] = "Error al cargar las alertas";

                // Asegurar que ViewBag tenga valores por defecto
                ViewBag.Alertas = new List<AlertaConsumoResponseDto>();
                ViewBag.Resumen = new ResumenAlertasDto();

                return View();
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            SetAuthHeader();
            try
            {
                var alerta = await _httpClient.GetFromJsonAsync<AlertaConsumoResponseDto>($"api/alertasconsumo/{id}");
                if (alerta == null) return NotFound();

                return View(alerta);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar detalle de alerta {Id}", id);
                TempData["Error"] = "Error al cargar la alerta";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        [RequierePermiso("ALERTAS_CONSUMO", "editar")]
        public async Task<IActionResult> Revisar(int id)
        {
            SetAuthHeader();
            try
            {
                var alerta = await _httpClient.GetFromJsonAsync<AlertaConsumoResponseDto>($"api/alertasconsumo/{id}");
                if (alerta == null) return NotFound();

                ViewBag.Estados = GetEstadosSelectList();
                return View(alerta);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar alerta para revisión {Id}", id);
                TempData["Error"] = "Error al cargar la alerta";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("ALERTAS_CONSUMO", "editar")]
        public async Task<IActionResult> Revisar(int id, EstadoAlerta nuevoEstado, string? observaciones)
        {
            SetAuthHeader();
            try
            {
                var dto = new { nuevoEstado, observaciones };
                var response = await _httpClient.PutAsJsonAsync($"api/alertasconsumo/{id}/revisar", dto);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Mensaje"] = "Alerta actualizada correctamente";
                    return RedirectToAction(nameof(Index));
                }

                TempData["Error"] = "Error al actualizar la alerta";
                return RedirectToAction(nameof(Revisar), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al revisar alerta {Id}", id);
                TempData["Error"] = "Error al actualizar la alerta";
                return RedirectToAction(nameof(Revisar), new { id });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("ALERTAS_CONSUMO", "editar")]
        public async Task<IActionResult> RevisarMasivo(List<int> idsAlertas, EstadoAlerta nuevoEstado, string? observaciones)
        {
            SetAuthHeader();
            try
            {
                var dto = new { idsAlertas, nuevoEstado, observaciones };
                var response = await _httpClient.PutAsJsonAsync("api/alertasconsumo/revisar-masivo", dto);

                if (response.IsSuccessStatusCode)
                    TempData["Mensaje"] = $"Alertas actualizadas correctamente";
                else
                    TempData["Error"] = "Error al actualizar las alertas";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al revisar alertas masivamente");
                TempData["Error"] = "Error al actualizar las alertas";
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Configuracion()
        {
            SetAuthHeader();
            try
            {
                var configuraciones = await _httpClient.GetFromJsonAsync<IEnumerable<ConfiguracionMaterialResponseDto>>(
                    "api/alertasconsumo/configuracion");

                return View(configuraciones);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar configuraciones");
                TempData["Error"] = "Error al cargar las configuraciones";
                return View(new List<ConfiguracionMaterialResponseDto>());
            }
        }

        [HttpGet]
        [RequierePermiso("CONFIG_MATERIALES", "crear")]
        public async Task<IActionResult> ConfigurarMaterial(int? idMaterial = null)
        {
            SetAuthHeader();
            await CargarMaterialesEnViewBag();

            if (idMaterial.HasValue)
            {
                var config = await _httpClient.GetFromJsonAsync<ConfiguracionMaterialResponseDto>(
                    $"api/alertasconsumo/configuracion/{idMaterial}");
                
                if (config != null)
                {
                    ViewBag.Configuracion = config;
                }
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("CONFIG_MATERIALES", "crear")]
        public async Task<IActionResult> ConfigurarMaterial(ConfiguracionMaterialRequestDto dto)
        {
            SetAuthHeader();
            if (!ModelState.IsValid)
            {
                await CargarMaterialesEnViewBag();
                return View();
            }

            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/alertasconsumo/configuracion", dto);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Mensaje"] = "Configuración guardada correctamente";
                    return RedirectToAction(nameof(Configuracion));
                }

                var error = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError("", error);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar configuración");
                ModelState.AddModelError("", "Error al guardar la configuración");
            }

            await CargarMaterialesEnViewBag();
            return View();
        }

        private async Task CargarFiltrosEnViewBag()
        {
            try
            {
                var proyectosResponse = await _httpClient.GetAsync("api/dashboardanalytics/filtros/proyectos");
                if (proyectosResponse.IsSuccessStatusCode)
                {
                    var proyectos = await proyectosResponse.Content.ReadFromJsonAsync<List<FiltroItemDto>>();
                    ViewBag.Proyectos = proyectos?.Select(p => new SelectListItem
                    {
                        Value = p.Id.ToString(),
                        Text = p.Nombre
                    }).ToList() ?? new List<SelectListItem>();
                }
                else
                {
                    ViewBag.Proyectos = new List<SelectListItem>();
                }

                ViewBag.Severidades = GetSeveridadesSelectList();
                ViewBag.Estados = GetEstadosSelectList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar filtros para ViewBag");
                ViewBag.Proyectos = new List<SelectListItem>();
                ViewBag.Severidades = GetSeveridadesSelectList();
                ViewBag.Estados = GetEstadosSelectList();
            }
        }

        private async Task CargarMaterialesEnViewBag()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/materiales");
                if (response.IsSuccessStatusCode)
                {
                    var materiales = await response.Content.ReadFromJsonAsync<IEnumerable<dynamic>>();
                    ViewBag.Materiales = materiales?.Select(m => new SelectListItem
                    {
                        Value = m.GetProperty("idMaterial").ToString(),
                        Text = m.GetProperty("nombre").GetString()
                    }).ToList() ?? new List<SelectListItem>();
                }
            }
            catch
            {
                ViewBag.Materiales = new List<SelectListItem>();
            }
        }

        private static List<SelectListItem> GetSeveridadesSelectList()
        {
            return new List<SelectListItem>
            {
                new() { Value = "1", Text = "Baja" },
                new() { Value = "2", Text = "Media" },
                new() { Value = "3", Text = "Alta" },
                new() { Value = "4", Text = "Crítica" }
            };
        }

        private static List<SelectListItem> GetEstadosSelectList()
        {
            return new List<SelectListItem>
            {
                new() { Value = "1", Text = "Pendiente" },
                new() { Value = "2", Text = "En Revisión" },
                new() { Value = "3", Text = "Descartada" },
                new() { Value = "4", Text = "Confirmada" },
                new() { Value = "5", Text = "Resuelta" }
            };
        }
    }
}
