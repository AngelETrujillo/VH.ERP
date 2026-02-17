using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VH.Services.DTOs.Analytics;
using VH.Web.Filters;

namespace VH.Web.Controllers
{
    [Authorize]
    [RequierePermiso("DASHBOARD_ANALYTICS", "ver")]
    public class DashboardAnalyticsController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<DashboardAnalyticsController> _logger;

        public DashboardAnalyticsController(IHttpClientFactory httpClientFactory, ILogger<DashboardAnalyticsController> logger)
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

        public async Task<IActionResult> Index(int? idProyecto = null)
        {
            SetAuthHeader();
            try
            {
                var anio = DateTime.Now.Year;
                var mes = DateTime.Now.Month;

                var kpisTask = _httpClient.GetFromJsonAsync<KPIsEjecutivosDto>($"api/dashboardanalytics/kpis?idProyecto={idProyecto}");
                var rankingTask = _httpClient.GetFromJsonAsync<RankingConsumoResponseDto>($"api/dashboardanalytics/ranking/{anio}/{mes}?idProyecto={idProyecto}");
                var alertasTask = _httpClient.GetFromJsonAsync<ResumenAlertasDto>($"api/alertasconsumo/resumen?idProyecto={idProyecto}");
                var tendenciaTask = _httpClient.GetFromJsonAsync<TendenciaConsumoDto>($"api/dashboardanalytics/tendencia-consumo?meses=6&idProyecto={idProyecto}");

                await Task.WhenAll(kpisTask, rankingTask, alertasTask, tendenciaTask);

                ViewBag.KPIs = await kpisTask;
                ViewBag.Ranking = await rankingTask;
                ViewBag.ResumenAlertas = await alertasTask;
                ViewBag.Tendencia = await tendenciaTask;
                ViewBag.Anio = anio;
                ViewBag.Mes = mes;

                await CargarFiltrosEnViewBag();
                ViewBag.FiltroProyecto = idProyecto;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar dashboard analytics");
                TempData["Error"] = "Error al cargar el dashboard";
                return View();
            }
        }

        public async Task<IActionResult> Ranking(int? anio = null, int? mes = null, int? idProyecto = null, int? idPuesto = null)
        {
            SetAuthHeader();
            try
            {
                anio ??= DateTime.Now.Year;
                mes ??= DateTime.Now.Month;

                var ranking = await _httpClient.GetFromJsonAsync<RankingConsumoResponseDto>(
                    $"api/dashboardanalytics/ranking/{anio}/{mes}?idProyecto={idProyecto}&idPuesto={idPuesto}");

                ViewBag.Ranking = ranking;
                ViewBag.Anio = anio;
                ViewBag.Mes = mes;

                await CargarFiltrosEnViewBag();
                ViewBag.FiltroProyecto = idProyecto;
                ViewBag.FiltroPuesto = idPuesto;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar ranking");
                TempData["Error"] = "Error al cargar el ranking";
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> PerfilEmpleado(int id)
        {
            SetAuthHeader();
            try
            {
                var perfil = await _httpClient.GetFromJsonAsync<PerfilConsumoEmpleadoDto>(
                    $"api/dashboardanalytics/perfil-empleado/{id}");

                if (perfil == null) return NotFound();

                return View(perfil);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar perfil del empleado {Id}", id);
                TempData["Error"] = "Error al cargar el perfil del empleado";
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> ConsumoProyectos(int? anio = null, int? mes = null)
        {
            SetAuthHeader();
            try
            {
                anio ??= DateTime.Now.Year;
                mes ??= DateTime.Now.Month;

                var consumo = await _httpClient.GetFromJsonAsync<IEnumerable<ConsumoProyectoDto>>(
                    $"api/dashboardanalytics/consumo-proyectos/{anio}/{mes}");

                ViewBag.Consumo = consumo;
                ViewBag.Anio = anio;
                ViewBag.Mes = mes;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar consumo por proyectos");
                TempData["Error"] = "Error al cargar el consumo por proyectos";
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> DetalleProyecto(int id, int? anio = null, int? mes = null)
        {
            SetAuthHeader();
            try
            {
                anio ??= DateTime.Now.Year;
                mes ??= DateTime.Now.Month;

                var detalle = await _httpClient.GetFromJsonAsync<ConsumoProyectoDto>(
                    $"api/dashboardanalytics/consumo-proyecto/{id}/{anio}/{mes}");

                var ranking = await _httpClient.GetFromJsonAsync<RankingConsumoResponseDto>(
                    $"api/dashboardanalytics/ranking/{anio}/{mes}?idProyecto={id}");

                var heatmap = await _httpClient.GetFromJsonAsync<HeatmapFrecuenciaDto>(
                    $"api/dashboardanalytics/heatmap/{id}/{anio}/{mes}");

                ViewBag.Detalle = detalle;
                ViewBag.Ranking = ranking;
                ViewBag.Heatmap = heatmap;
                ViewBag.Anio = anio;
                ViewBag.Mes = mes;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar detalle del proyecto {Id}", id);
                TempData["Error"] = "Error al cargar el detalle del proyecto";
                return RedirectToAction(nameof(ConsumoProyectos));
            }
        }

        [HttpPost]
        [RequierePermiso("DASHBOARD_ANALYTICS", "editar")]
        public async Task<IActionResult> RecalcularEstadisticas(int anio, int mes)
        {
            SetAuthHeader();
            try
            {
                var response = await _httpClient.PostAsync(
                    $"api/dashboardanalytics/recalcular-estadisticas/{anio}/{mes}", null);

                if (response.IsSuccessStatusCode)
                    TempData["Mensaje"] = "Estadísticas recalculadas correctamente";
                else
                    TempData["Error"] = "Error al recalcular estadísticas";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al recalcular estadísticas");
                TempData["Error"] = "Error al recalcular estadísticas";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetTendenciaJson(int meses = 12, int? idProyecto = null)
        {
            SetAuthHeader();
            var tendencia = await _httpClient.GetFromJsonAsync<TendenciaConsumoDto>(
                $"api/dashboardanalytics/tendencia-consumo?meses={meses}&idProyecto={idProyecto}");
            return Json(tendencia);
        }

        [HttpGet]
        public async Task<IActionResult> GetHeatmapJson(int idProyecto, int anio, int mes)
        {
            SetAuthHeader();
            var heatmap = await _httpClient.GetFromJsonAsync<HeatmapFrecuenciaDto>(
                $"api/dashboardanalytics/heatmap/{idProyecto}/{anio}/{mes}");
            return Json(heatmap);
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

                var puestosResponse = await _httpClient.GetAsync("api/dashboardanalytics/filtros/puestos");
                if (puestosResponse.IsSuccessStatusCode)
                {
                    var puestos = await puestosResponse.Content.ReadFromJsonAsync<List<FiltroItemDto>>();
                    ViewBag.Puestos = puestos?.Select(p => new SelectListItem
                    {
                        Value = p.Id.ToString(),
                        Text = p.Nombre
                    }).ToList() ?? new List<SelectListItem>();
                }
                else
                {
                    ViewBag.Puestos = new List<SelectListItem>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar filtros para ViewBag");
                ViewBag.Proyectos = new List<SelectListItem>();
                ViewBag.Puestos = new List<SelectListItem>();
            }
        }
    }
}
