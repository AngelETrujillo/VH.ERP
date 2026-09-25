using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VH.Services.DTOs;
using VH.Web.Filters;

namespace VH.Web.Controllers
{
    [Authorize]
    [RequierePermiso("PROYECTOS", "ver")]
    public class ProyectosController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ProyectosController> _logger;

        public ProyectosController(IHttpClientFactory httpClientFactory, ILogger<ProyectosController> logger)
        {
            _httpClient = httpClientFactory.CreateClient("ApiERP");
            _logger = logger;
        }

        public async Task<IActionResult> Index(
            int pagina = 1, int tamano = ConsultaPaginada.TamanoPorOmision, string? buscar = null)
        {
            var consulta = new ConsultaPaginada { Pagina = pagina, Tamano = tamano, Buscar = buscar };

            try
            {
                var url = $"api/proyectos/paginado?pagina={consulta.Pagina}&tamano={consulta.Tamano}";
                if (consulta.HayBusqueda) url += $"&buscar={Uri.EscapeDataString(consulta.TextoLimpio!)}";

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var pag = await response.Content.ReadFromJsonAsync<ResultadoPaginado<ProyectoResponseDto>>();
                return View(pag ?? ResultadoPaginado<ProyectoResponseDto>.Ninguno(consulta));
            }
            catch (HttpRequestException e)
            {
                _logger.LogError(e, "Error al conectar con la API.");
                ViewBag.ErrorMessage = $"Error al cargar proyectos: {e.Message}";
                return View(ResultadoPaginado<ProyectoResponseDto>.Ninguno(consulta));
            }
        }

        [RequierePermiso("PROYECTOS", "crear")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("PROYECTOS", "crear")]
        public async Task<IActionResult> Create([Bind("Nombre,TipoObra,FechaInicio,FechaFinEstimada,PresupuestoTotal,PresupuestoEPPMensual")] ProyectoRequestDto proyectoDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var response = await _httpClient.PostAsJsonAsync("api/proyectos", proyectoDto);
                    response.EnsureSuccessStatusCode();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al crear el proyecto: " + ex.Message);
                }
            }
            return View(proyectoDto);
        }

        [RequierePermiso("PROYECTOS", "editar")]
        public async Task<IActionResult> Edit(int id)
        {
            var response = await _httpClient.GetAsync($"api/proyectos/{id}");

            if (!response.IsSuccessStatusCode)
                return NotFound();

            var proyectoResponse = await response.Content.ReadFromJsonAsync<ProyectoResponseDto>();

            var proyectoRequest = new ProyectoRequestDto(
                proyectoResponse!.Nombre,
                proyectoResponse.TipoObra,
                proyectoResponse.FechaInicio,
                proyectoResponse.FechaFinEstimada,
                proyectoResponse.PresupuestoTotal,
                proyectoResponse.PresupuestoEPPMensual
            );

            return View(proyectoRequest);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequierePermiso("PROYECTOS", "editar")]
        public async Task<IActionResult> Edit(int id, [Bind("Nombre,TipoObra,FechaInicio,FechaFinEstimada,PresupuestoTotal,PresupuestoEPPMensual")] ProyectoRequestDto proyectoDto)
        {
            if (id <= 0)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var response = await _httpClient.PutAsJsonAsync($"api/proyectos/{id}", proyectoDto);
                    response.EnsureSuccessStatusCode();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al actualizar el proyecto: " + ex.Message);
                }
            }
            return View(proyectoDto);
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/proyectos/{id}");
                if (!response.IsSuccessStatusCode) return NotFound();
                var proyecto = await response.Content.ReadFromJsonAsync<ProyectoResponseDto>();
                return View(proyecto);
            }
            catch
            {
                return NotFound();
            }
        }

        [RequierePermiso("PROYECTOS", "eliminar")]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _httpClient.GetAsync($"api/proyectos/{id}");

            if (!response.IsSuccessStatusCode)
                return NotFound();

            var proyecto = await response.Content.ReadFromJsonAsync<ProyectoResponseDto>();
            return View(proyecto);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [RequierePermiso("PROYECTOS", "eliminar")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _httpClient.DeleteAsync($"api/proyectos/{id}");
            return RedirectToAction(nameof(Index));
        }
    }
}